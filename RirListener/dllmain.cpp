
#include "pch.h"
#include <iostream>

struct InterceptorKeyboardInput
{
    DWORD m_nVirtualKey;
    DWORD m_nProcessId;
    BOOL m_bPeek;
    DWORD m_OriginalWindowHandle;
};

struct InterceptorCBT {
    DWORD Code;
    DWORD m_nProcessId;
};

struct InterceptorData32
{
    DWORD m_KeyboardHookHandle = 0;
    DWORD m_CBTHookHandle = 0;
};

struct InterceptorData64
{
    DWORD m_KeyboardHookHandle = 0;
    DWORD m_CBTHookHandle = 0;
};

struct InterceptorSharedData
{
    DWORD m_ReceiverWindowHandle = 0;
    InterceptorKeyboardInput m_Keyboard = { 0 };
    InterceptorCBT m_CBT = { 0 };
    InterceptorData32 m_Data32 = { 0 };
    InterceptorData64 m_Data64 = { 0 };
};

/*
    NOTE: Make sure all shared global variables are initialized, otherwise vars are stored in .bss section instead.
    Read: https://docs.microsoft.com/en-us/cpp/preprocessor/data-seg?view=vs-2019
*/

// TODO: Move this to a shared memory file mapping for 32-bit and 64-bit interop
#pragma comment(linker, "/section:.shared,RWS")
#pragma data_seg(".shared")

InterceptorSharedData m_SharedData = { 0 };

#pragma data_seg()

bool g_bInitialized = false;
HMODULE g_hMyHandle = NULL;

#define WM_HOOK_KEYBOARD_INTERCEPT (WM_APP+0)
#define WM_HOOK_CBT (WM_APP+1)
#define WM_DEBUG_OUTPUT (WM_APP+2)
#define WM_HHELPER_INSTALL (WM_APP+3)
#define WM_HHELPER_UNINSTALL (WM_APP+4)

void SendDebugMessage(DWORD type, DWORD result)
{
#ifdef _DEBUG
    HWND hWindow = (HWND)UIntToPtr(m_SharedData.m_ReceiverWindowHandle);
    if (!IsWindow(hWindow))
        return;

    PostMessage(hWindow, WM_DEBUG_OUTPUT, type, result);
#endif
}

LRESULT WINAPI KeyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    HWND hWindow = (HWND)UIntToPtr(m_SharedData.m_ReceiverWindowHandle);

    if (nCode >= 0 && IsWindow(hWindow))
    {
        InterceptorKeyboardInput* pKeyboard = &m_SharedData.m_Keyboard;

        pKeyboard->m_bPeek = nCode == HC_NOREMOVE;
        pKeyboard->m_nProcessId = GetCurrentProcessId();
        pKeyboard->m_OriginalWindowHandle = PtrToUint(GetFocus());
        pKeyboard->m_nVirtualKey = (int)wParam;

        // Try to get a response from the program. If result is non-zero, will block the input.
        LRESULT result = SendMessage(hWindow, WM_HOOK_KEYBOARD_INTERCEPT, wParam, lParam);
#if 0
        SendDebugMessage(1, (DWORD)result);
#endif
        if (result)
        {
            return 1;
        }
    }

    return CallNextHookEx((HHOOK)UintToPtr(m_SharedData.m_Data64.m_KeyboardHookHandle), nCode, wParam, lParam);
}

LRESULT WINAPI CBTProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    HWND hWindow = (HWND)UintToPtr(m_SharedData.m_ReceiverWindowHandle);

    if (nCode >= 0 && IsWindow(hWindow))
    {
        InterceptorCBT* pCBT = &m_SharedData.m_CBT;

        pCBT->Code = nCode;
        pCBT->m_nProcessId = GetCurrentProcessId();
        PostMessageW(hWindow, WM_HOOK_CBT, wParam, lParam);
    }

    return CallNextHookEx((HHOOK)UintToPtr(m_SharedData.m_Data64.m_CBTHookHandle), nCode, wParam, lParam);
}

// extern "C" is used to prevent name mangling from the compiler.
extern "C" {
    __declspec(dllexport) void __stdcall RirGetKeyboardInput(InterceptorKeyboardInput &inputData) {
        inputData = m_SharedData.m_Keyboard;
    }

    __declspec(dllexport) void __stdcall RirGetCBT(InterceptorCBT &cbtData) {
        cbtData = m_SharedData.m_CBT;
    }

    __declspec(dllexport) bool __stdcall RirUninstall() {
        if (!g_bInitialized)
            return true;

        InterceptorData64* pData = &m_SharedData.m_Data64;

        if (pData->m_KeyboardHookHandle) {
            UnhookWindowsHookEx((HHOOK)UIntToPtr(pData->m_KeyboardHookHandle));
            pData->m_KeyboardHookHandle = NULL;
        }

        if (pData->m_CBTHookHandle) {
            UnhookWindowsHookEx((HHOOK)UIntToPtr(pData->m_CBTHookHandle));
            pData->m_CBTHookHandle = NULL;
        }

        m_SharedData.m_ReceiverWindowHandle = NULL;

        g_bInitialized = false;

        return true;
    }

    __declspec(dllexport) bool __stdcall RirInstall(HWND hWnd) {
        if (g_bInitialized)
            return true;

        if (!IsWindow(hWnd)) {
            return false;
        }

        m_SharedData.m_ReceiverWindowHandle = PtrToUint(hWnd);

        HHOOK hHook = SetWindowsHookExW(WH_KEYBOARD, KeyboardHookProc, g_hMyHandle, 0);
        if (!hHook) {
            return false;
        }

        m_SharedData.m_Data64.m_KeyboardHookHandle = PtrToUint(hHook);

        hHook = SetWindowsHookExW(WH_CBT, CBTProc, g_hMyHandle, 0);
        if (!hHook) {
            return false;
        }

        m_SharedData.m_Data64.m_CBTHookHandle = PtrToUint(hHook);

        g_bInitialized = true;

        return true;
    }

    __declspec(dllexport) HMODULE __stdcall RirGetModuleHandle() {
        return g_hMyHandle;
    }
}

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hMyHandle = hModule;
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}