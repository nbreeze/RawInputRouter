using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RawInputRouter.Imports
{
    public sealed partial class User32
    {
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public static bool EnumWindows(Predicate<IntPtr> predicate)
        {
            return EnumWindows((hWnd, lParam) => predicate(hWnd), IntPtr.Zero);
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITTHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetGUIThreadInfo(int idThread, IntPtr pgui);

        public static bool GetGUIThreadInfo(int idThread, ref GUITTHREADINFO threadInfo)
        {
            int structSize = Marshal.SizeOf<GUITTHREADINFO>();
            threadInfo.cbSize = structSize;

            var ptr = Marshal.AllocHGlobal(structSize);
            if (ptr == IntPtr.Zero)
                return false;

            Marshal.StructureToPtr(threadInfo, ptr, false);

            var result = GetGUIThreadInfo(idThread, ptr);
            if (result)
            {
                var structNew = Marshal.PtrToStructure<GUITTHREADINFO>(ptr);
                threadInfo.hwndActive = structNew.hwndActive;
                threadInfo.hwndCapture = structNew.hwndCapture;
                threadInfo.hwndCaret = structNew.hwndCaret;
                threadInfo.hwndFocus = structNew.hwndFocus;
                threadInfo.hwndMenuOwner = structNew.hwndMenuOwner;
                threadInfo.hwndMoveSize = structNew.hwndMoveSize;
                threadInfo.rcCaret = structNew.rcCaret;
            }
            Marshal.FreeHGlobal(ptr);

            return result;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public enum RawInputType
        {
            Mouse = 0,
            Keyboard = 1,
            HumanInterfaceDevice = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterRawInputDevices(IntPtr pRawInputDevices, int uiNumDevices, int cbSize);

        public static bool RegisterRawInputDevices(IList<RAWINPUTDEVICE> rawInputDevices)
        {
            if (rawInputDevices.Count == 0)
                return true;

            int structSize = Marshal.SizeOf<RAWINPUTDEVICE>();

            IntPtr mem = Marshal.AllocHGlobal(structSize * rawInputDevices.Count);
            if (mem == IntPtr.Zero) return false;

            for (int i = 0; i < rawInputDevices.Count; i++)
            {
                var device = rawInputDevices[i];
                var addr = IntPtr.Add(mem, structSize * i);
                Marshal.StructureToPtr(device, addr, false);
            }

            bool result = RegisterRawInputDevices(mem, rawInputDevices.Count, structSize);

            Marshal.FreeHGlobal(mem);
            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint puiNumDevices, int cbSize);

        public static uint GetRawInputDeviceList(IList<RAWINPUTDEVICELIST> devices)
        {
            devices.Clear();

            uint uNumDevices = 0;
            int structSize = Marshal.SizeOf<RAWINPUTDEVICELIST>();

            unsafe
            {
                GetRawInputDeviceList(IntPtr.Zero, ref uNumDevices, structSize);
            }

            uint ret = 0;

            if (uNumDevices > 0)
            {
                IntPtr ptr = Marshal.AllocHGlobal(structSize * (int)uNumDevices);

                unsafe
                {
                    ret = GetRawInputDeviceList(ptr, ref uNumDevices, structSize);
                }

                for (int i = 0; i < uNumDevices; i++)
                {
                    RAWINPUTDEVICELIST rd = Marshal.PtrToStructure<RAWINPUTDEVICELIST>(IntPtr.Add(ptr, structSize * i));
                    devices.Add(rd);
                }

                Marshal.FreeHGlobal(ptr);
            }

            return ret;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public int Message;
            public UIntPtr ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWMOUSE
        {
            public ushort usFlags;
            public UIntPtr ulButtons;
            public UIntPtr ulRawButtons;
            public IntPtr lLastX;
            public IntPtr lLastY;
            public UIntPtr ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWHID
        {
            public uint dwSizeHid;
            public uint dwCount;
            public byte bRawData;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWINPUT32
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(sizeof(int) * 2 + sizeof(int) * 2)]
            public RAWMOUSE mouse;
            [FieldOffset(sizeof(int) * 2 + sizeof(int) * 2)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(sizeof(int) * 2 + sizeof(int) * 2)]
            public RAWHID hid;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWINPUT64
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(sizeof(int) * 2 + sizeof(long) * 2)]
            public RAWMOUSE mouse;
            [FieldOffset(sizeof(int) * 2 + sizeof(long) * 2)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(sizeof(int) * 2 + sizeof(long) * 2)]
            public RAWHID hid;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref int pcbSize, int cbSizeHeader);

        public static bool GetRawInputData(IntPtr hRawInput,
            ref RAWINPUTHEADER header,
            ref RAWMOUSE mouse,
            ref RAWKEYBOARD keyboard,
            ref RAWHID hid)
        {
            int iSize = 0;
            GetRawInputData(hRawInput, 0x10000003, IntPtr.Zero, ref iSize, Marshal.SizeOf<RAWINPUTHEADER>()); // RID_INPUT
            if (iSize == 0)
                return false;

            IntPtr mem = Marshal.AllocHGlobal(iSize);
            if (mem == IntPtr.Zero)
                return false;

            if (GetRawInputData(hRawInput, 0x10000003, mem, ref iSize, Marshal.SizeOf<RAWINPUTHEADER>()) != iSize)
            {
                Marshal.FreeHGlobal(mem);
                return false;
            }

            if (Environment.Is64BitProcess)
            {
                var str = Marshal.PtrToStructure<RAWINPUT64>(mem);
                header = str.header;
                mouse = str.mouse;
                keyboard = str.keyboard;
                hid = str.hid;
            }
            else
            {
                var str = Marshal.PtrToStructure<RAWINPUT32>(mem);
                header = str.header;
                mouse = str.mouse;
                keyboard = str.keyboard;
                hid = str.hid;
            }

            Marshal.FreeHGlobal(mem);
            return true;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        public static extern int GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        public static string GetRawInputDeviceName(IntPtr hDevice, uint maxChars = 260)
        {
            uint ptrSize = maxChars * 2;
            IntPtr ptr = Marshal.AllocHGlobal((int)ptrSize);

            string name = null;
            if (GetRawInputDeviceInfo(hDevice, 0x20000007, ptr, ref ptrSize) > 0) // RIDI_DEVICENAME
            {
                name = Marshal.PtrToStringUni(ptr);
            }

            Marshal.FreeHGlobal(ptr);

            return name;
        }
    }
}
