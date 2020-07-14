
#include "pch.h"

struct RirKeyboardInput {
    bool Peek;
    unsigned int ProcessId;
    HWND Window;
    unsigned int VKey;
};

struct RirCBT {
    int Code;
    unsigned int ProcessId;
};

/*
    NOTE: Make sure all shared global variables are initialized, otherwise vars are stored in .bss section instead.
    Read: https://docs.microsoft.com/en-us/cpp/preprocessor/data-seg?view=vs-2019
*/

#pragma comment(linker, "/section:.shared,RWS")
#pragma data_seg(".shared")

HWND g_hWindow = NULL;
HHOOK g_hKeyboardHook = NULL;
HHOOK g_hCBTHook = NULL;
RirKeyboardInput g_KeyboardProcInfo = { 0 };
RirCBT g_CBTProcInfo = { 0 };

#pragma data_seg()

bool g_bInitialized = false;
HMODULE g_hMyHandle = NULL;

#define WM_HOOK_KEYBOARD 0xFACE
#define WM_HOOK_CBT 0xdabb

LRESULT WINAPI KeyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
    if (nCode >= 0 && IsWindow(g_hWindow)) {
        g_KeyboardProcInfo.Peek = nCode == HC_NOREMOVE;
        g_KeyboardProcInfo.ProcessId = GetCurrentProcessId();
        g_KeyboardProcInfo.Window = GetFocus();
        g_KeyboardProcInfo.VKey = wParam;

        if (SendMessageW(g_hWindow, WM_HOOK_KEYBOARD, wParam, lParam))
        {
            return 1;
        }
    }

    return CallNextHookEx(g_hKeyboardHook, nCode, wParam, lParam);
}

LRESULT WINAPI CBTProc(int nCode, WPARAM wParam, LPARAM lParam) {

    if (nCode >= 0 && IsWindow(g_hWindow))
    {
        g_CBTProcInfo.Code = nCode;
        g_CBTProcInfo.ProcessId = GetCurrentProcessId();
        SendMessageW(g_hWindow, WM_HOOK_CBT, wParam, lParam);
    }

    return CallNextHookEx(g_hCBTHook, nCode, wParam, lParam);
}

extern "C" {
    __declspec(dllexport) void __stdcall RirGetKeyboardInput(unsigned int * pVKey, unsigned int * pProcId, HWND * pWindow, bool* pPeek ) {
        if (pVKey)
            *pVKey = g_KeyboardProcInfo.VKey;
        if (pProcId)
            *pProcId = g_KeyboardProcInfo.ProcessId;
        if (pWindow)
            *pWindow = g_KeyboardProcInfo.Window;
        if (pPeek)
            *pPeek = g_KeyboardProcInfo.Peek;
    }

    __declspec(dllexport) void __stdcall RirGetCBT(int* pCode, unsigned int* pProcId) {
        if (pCode)
            *pCode = g_CBTProcInfo.Code;
        if (pProcId)
            *pProcId = g_CBTProcInfo.ProcessId;
    }

    __declspec(dllexport) bool __stdcall RirUninstall() {
        if (!g_bInitialized)
            return true;

        if (g_hKeyboardHook) {
            UnhookWindowsHookEx(g_hKeyboardHook);
            g_hKeyboardHook = NULL;
        }

        if (g_hCBTHook) {
            UnhookWindowsHookEx(g_hCBTHook);
            g_hCBTHook = NULL;
        }

        g_hWindow = NULL;

        g_bInitialized = false;

        return true;
    }

    __declspec(dllexport) bool __stdcall RirInstall(HWND hWnd) {
        if (g_bInitialized)
            return true;

        if (!IsWindow(hWnd)) {
            return false;
        }

        g_hWindow = hWnd;

        g_hKeyboardHook = SetWindowsHookExW(WH_KEYBOARD, KeyboardHookProc, g_hMyHandle, 0);
        if (!g_hKeyboardHook) {
            return false;
        }

        g_hCBTHook = SetWindowsHookExW(WH_CBT, CBTProc, g_hMyHandle, 0);
        if (!g_hCBTHook) {
            return false;
        }

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