using System;
using System.Runtime.InteropServices;

namespace RawInputRouter.Imports
{
    public static class WinMsgIntercept
    {
        public const int WM_HOOK_KEYBOARD_INTERCEPT = User32.WM_APP + 0;
        public const int WM_HOOK_CBT = User32.WM_APP + 1;
        public const int WM_DEBUG_OUTPUT = User32.WM_APP + 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public int m_nVirtualKey;
            public int m_nProcessId;
            public int m_bPeek;
            public int m_OriginalWindowHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CBT
        {
            public int Code;
            public int m_nProcessId;
        }

        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirInstall")]
        public static extern bool Install(IntPtr hWnd);

        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirUninstall")]
        public static extern bool Uninstall();

        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetKeyboardInput")]
        public static extern bool GetKeyboardInput(ref KeyboardInput pInput);

        [DllImport("WinMsgInterceptx64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetCBT")]
        public static extern bool GetCBT(ref CBT pCBT);
    }
}
