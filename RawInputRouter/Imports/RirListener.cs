using System;
using System.Runtime.InteropServices;

namespace RawInputRouter.Imports
{
    public class RirListener
    {
        [DllImport("RirListener.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirInstall")]
        public static extern bool Install(IntPtr hWnd);

        [DllImport("RirListener.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirUninstall")]
        public static extern bool Uninstall();

        [DllImport("RirListener.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetKeyboardInput")]
        public static extern bool GetKeyboardInput(ref uint pVKey, ref uint pProcId, ref IntPtr pWindow, ref bool pPeek);

        [DllImport("RirListener.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "RirGetCBT")]
        public static extern bool GetCBT(ref int pCode, ref uint pProcId);
    }
}
