using System;
using System.Runtime.InteropServices;
using PInvoke;

namespace Redirector.Native
{
    public static class WinMsgIntercept
    {
        public const int WM_HOOK_KEYBOARD_INTERCEPT = (int)User32.WindowMessage.WM_APP + 0;
        public const int WM_HOOK_CBT = WM_HOOK_KEYBOARD_INTERCEPT + 1;
        public const int WM_DEBUG_OUTPUT = WM_HOOK_KEYBOARD_INTERCEPT + 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public int m_nVirtualKey;
            public int m_nProcessId;
            public int m_bPeek;
            public uint m_OriginalWindowHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CBT
        {
            public int Code;
            public int m_nProcessId;
        }

#if WIN64
        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirInstall")]
#else
        [DllImport("WinMsgInterceptx32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirInstall")]
#endif
        public static extern bool Install(IntPtr hWnd);

#if WIN64
        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirUninstall")]
#else
        [DllImport("WinMsgInterceptx32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirUninstall")]
#endif
        public static extern bool Uninstall();

#if WIN64
        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetKeyboardInput")]
#else
        [DllImport("WinMsgInterceptx32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetKeyboardInput")]
#endif
        public static extern bool GetKeyboardInput(out KeyboardInput pInput);

#if WIN64
        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetCBT")]
#else
        [DllImport("WinMsgInterceptx32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetKeyboardInput")]
#endif
        public static extern bool GetCBT(out CBT pCBT);
    }
}
