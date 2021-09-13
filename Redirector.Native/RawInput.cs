using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Redirector.Native
{
    public static class RawInput
    {
        public const int RID_HEADER = 0x10000005; // Get the header information from the RAWINPUT structure.
        public const int RID_INPUT = 0x10000003; // Get the raw data from the RAWINPUT structure.

        /// <summary>
        /// Enumeration containing the type device the raw input is coming from.
        /// </summary>
        public enum RawInputType : uint
        {
            /// <summary>
            /// Mouse input.
            /// </summary>
            Mouse = 0,

            /// <summary>
            /// Keyboard input.
            /// </summary>
            Keyboard = 1,

            /// <summary>
            ///  Human interface device input.
            /// </summary>
            HID = 2,

            /// <summary>
            /// Another device that is not the keyboard or the mouse.
            /// </summary>
            Other = 3
        }

        internal enum DeviceInfoTypes : uint
        {
            RIDI_PREPARSEDDATA = 0x20000005, // pData is a PHIDP_PREPARSED_DATA pointer to a buffer for a top-level collection's preparsed data.
            RIDI_DEVICENAME = 0x20000007, // pData points to a string that contains the device interface name. For this uiCommand only, the value in pcbSize is the character count (not the byte count).
            RIDI_DEVICEINFO = 0x2000000b // pData points to an RID_DEVICE_INFO structure.
        };

        public const int RI_KEY_MAKE = 0; // Key is up
        public const int RI_KEY_BREAK = 1; // Key is down
        public const int RI_KEY_E0 = 2; // The scan code has the E0 prefix.
        public const int RI_KEY_E1 = 4; // The scan code has the E1 prefix.

        /// <summary>
        /// Enumeration containing HID usage page flags.
        /// </summary>
        public enum HIDUsagePage : ushort
        {
            /// <summary>Unknown usage page.</summary>
            Undefined = 0x00,
            /// <summary>Generic desktop controls.</summary>
            Generic = 0x01,
            /// <summary>Simulation controls.</summary>
            Simulation = 0x02,
            /// <summary>Virtual reality controls.</summary>
            VR = 0x03,
            /// <summary>Sports controls.</summary>
            Sport = 0x04,
            /// <summary>Games controls.</summary>
            Game = 0x05,
            /// <summary>Keyboard controls.</summary>
            Keyboard = 0x07,
            /// <summary>LED controls.</summary>
            LED = 0x08,
            /// <summary>Button.</summary>
            Button = 0x09,
            /// <summary>Ordinal.</summary>
            Ordinal = 0x0A,
            /// <summary>Telephony.</summary>
            Telephony = 0x0B,
            /// <summary>Consumer.</summary>
            Consumer = 0x0C,
            /// <summary>Digitizer.</summary>
            Digitizer = 0x0D,
            /// <summary>Physical interface device.</summary>
            PID = 0x0F,
            /// <summary>Unicode.</summary>
            Unicode = 0x10,
            /// <summary>Alphanumeric display.</summary>
            AlphaNumeric = 0x14,
            /// <summary>Medical instruments.</summary>
            Medical = 0x40,
            /// <summary>Monitor page 0.</summary>
            MonitorPage0 = 0x80,
            /// <summary>Monitor page 1.</summary>
            MonitorPage1 = 0x81,
            /// <summary>Monitor page 2.</summary>
            MonitorPage2 = 0x82,
            /// <summary>Monitor page 3.</summary>
            MonitorPage3 = 0x83,
            /// <summary>Power page 0.</summary>
            PowerPage0 = 0x84,
            /// <summary>Power page 1.</summary>
            PowerPage1 = 0x85,
            /// <summary>Power page 2.</summary>
            PowerPage2 = 0x86,
            /// <summary>Power page 3.</summary>
            PowerPage3 = 0x87,
            /// <summary>Bar code scanner.</summary>
            BarCode = 0x8C,
            /// <summary>Scale page.</summary>
            Scale = 0x8D,
            /// <summary>Magnetic strip reading devices.</summary>
            MSR = 0x8E
        }

        /// <summary>
        /// Enumeration containing the HID usage values.
        /// </summary>
        public enum HIDUsage : ushort
        {
            /// <summary></summary>
            Pointer = 0x01,
            /// <summary></summary>
            Mouse = 0x02,
            /// <summary></summary>
            Joystick = 0x04,
            /// <summary></summary>
            Gamepad = 0x05,
            /// <summary></summary>
            Keyboard = 0x06,
            /// <summary></summary>
            Keypad = 0x07,
            /// <summary></summary>
            SystemControl = 0x80,
            /// <summary></summary>
            X = 0x30,
            /// <summary></summary>
            Y = 0x31,
            /// <summary></summary>
            Z = 0x32,
            /// <summary></summary>
            RelativeX = 0x33,
            /// <summary></summary>    
            RelativeY = 0x34,
            /// <summary></summary>
            RelativeZ = 0x35,
            /// <summary></summary>
            Slider = 0x36,
            /// <summary></summary>
            Dial = 0x37,
            /// <summary></summary>
            Wheel = 0x38,
            /// <summary></summary>
            HatSwitch = 0x39,
            /// <summary></summary>
            CountedBuffer = 0x3A,
            /// <summary></summary>
            ByteCount = 0x3B,
            /// <summary></summary>
            MotionWakeup = 0x3C,
            /// <summary></summary>
            VX = 0x40,
            /// <summary></summary>
            VY = 0x41,
            /// <summary></summary>
            VZ = 0x42,
            /// <summary></summary>
            VBRX = 0x43,
            /// <summary></summary>
            VBRY = 0x44,
            /// <summary></summary>
            VBRZ = 0x45,
            /// <summary></summary>
            VNO = 0x46,
            /// <summary></summary>
            SystemControlPower = 0x81,
            /// <summary></summary>
            SystemControlSleep = 0x82,
            /// <summary></summary>
            SystemControlWake = 0x83,
            /// <summary></summary>
            SystemControlContextMenu = 0x84,
            /// <summary></summary>
            SystemControlMainMenu = 0x85,
            /// <summary></summary>
            SystemControlApplicationMenu = 0x86,
            /// <summary></summary>
            SystemControlHelpMenu = 0x87,
            /// <summary></summary>
            SystemControlMenuExit = 0x88,
            /// <summary></summary>
            SystemControlMenuSelect = 0x89,
            /// <summary></summary>
            SystemControlMenuRight = 0x8A,
            /// <summary></summary>
            SystemControlMenuLeft = 0x8B,
            /// <summary></summary>
            SystemControlMenuUp = 0x8C,
            /// <summary></summary>
            SystemControlMenuDown = 0x8D,
            /// <summary></summary>
            KeyboardNoEvent = 0x00,
            /// <summary></summary>
            KeyboardRollover = 0x01,
            /// <summary></summary>
            KeyboardPostFail = 0x02,
            /// <summary></summary>
            KeyboardUndefined = 0x03,
            /// <summary></summary>
            KeyboardaA = 0x04,
            /// <summary></summary>
            KeyboardzZ = 0x1D,
            /// <summary></summary>
            Keyboard1 = 0x1E,
            /// <summary></summary>
            Keyboard0 = 0x27,
            /// <summary></summary>
            KeyboardLeftControl = 0xE0,
            /// <summary></summary>
            KeyboardLeftShift = 0xE1,
            /// <summary></summary>
            KeyboardLeftALT = 0xE2,
            /// <summary></summary>
            KeyboardLeftGUI = 0xE3,
            /// <summary></summary>
            KeyboardRightControl = 0xE4,
            /// <summary></summary>
            KeyboardRightShift = 0xE5,
            /// <summary></summary>
            KeyboardRightALT = 0xE6,
            /// <summary></summary>
            KeyboardRightGUI = 0xE7,
            /// <summary></summary>
            KeyboardScrollLock = 0x47,
            /// <summary></summary>
            KeyboardNumLock = 0x53,
            /// <summary></summary>
            KeyboardCapsLock = 0x39,
            /// <summary></summary>
            KeyboardF1 = 0x3A,
            /// <summary></summary>
            KeyboardF12 = 0x45,
            /// <summary></summary>
            KeyboardReturn = 0x28,
            /// <summary></summary>
            KeyboardEscape = 0x29,
            /// <summary></summary>
            KeyboardDelete = 0x2A,
            /// <summary></summary>
            KeyboardPrintScreen = 0x46,
            /// <summary></summary>
            LEDNumLock = 0x01,
            /// <summary></summary>
            LEDCapsLock = 0x02,
            /// <summary></summary>
            LEDScrollLock = 0x03,
            /// <summary></summary>
            LEDCompose = 0x04,
            /// <summary></summary>
            LEDKana = 0x05,
            /// <summary></summary>
            LEDPower = 0x06,
            /// <summary></summary>
            LEDShift = 0x07,
            /// <summary></summary>
            LEDDoNotDisturb = 0x08,
            /// <summary></summary>
            LEDMute = 0x09,
            /// <summary></summary>
            LEDToneEnable = 0x0A,
            /// <summary></summary>
            LEDHighCutFilter = 0x0B,
            /// <summary></summary>
            LEDLowCutFilter = 0x0C,
            /// <summary></summary>
            LEDEqualizerEnable = 0x0D,
            /// <summary></summary>
            LEDSoundFieldOn = 0x0E,
            /// <summary></summary>
            LEDSurroundFieldOn = 0x0F,
            /// <summary></summary>
            LEDRepeat = 0x10,
            /// <summary></summary>
            LEDStereo = 0x11,
            /// <summary></summary>
            LEDSamplingRateDirect = 0x12,
            /// <summary></summary>
            LEDSpinning = 0x13,
            /// <summary></summary>
            LEDCAV = 0x14,
            /// <summary></summary>
            LEDCLV = 0x15,
            /// <summary></summary>
            LEDRecordingFormatDet = 0x16,
            /// <summary></summary>
            LEDOffHook = 0x17,
            /// <summary></summary>
            LEDRing = 0x18,
            /// <summary></summary>
            LEDMessageWaiting = 0x19,
            /// <summary></summary>
            LEDDataMode = 0x1A,
            /// <summary></summary>
            LEDBatteryOperation = 0x1B,
            /// <summary></summary>
            LEDBatteryOK = 0x1C,
            /// <summary></summary>
            LEDBatteryLow = 0x1D,
            /// <summary></summary>
            LEDSpeaker = 0x1E,
            /// <summary></summary>
            LEDHeadset = 0x1F,
            /// <summary></summary>
            LEDHold = 0x20,
            /// <summary></summary>
            LEDMicrophone = 0x21,
            /// <summary></summary>
            LEDCoverage = 0x22,
            /// <summary></summary>
            LEDNightMode = 0x23,
            /// <summary></summary>
            LEDSendCalls = 0x24,
            /// <summary></summary>
            LEDCallPickup = 0x25,
            /// <summary></summary>
            LEDConference = 0x26,
            /// <summary></summary>
            LEDStandBy = 0x27,
            /// <summary></summary>
            LEDCameraOn = 0x28,
            /// <summary></summary>
            LEDCameraOff = 0x29,
            /// <summary></summary>    
            LEDOnLine = 0x2A,
            /// <summary></summary>
            LEDOffLine = 0x2B,
            /// <summary></summary>
            LEDBusy = 0x2C,
            /// <summary></summary>
            LEDReady = 0x2D,
            /// <summary></summary>
            LEDPaperOut = 0x2E,
            /// <summary></summary>
            LEDPaperJam = 0x2F,
            /// <summary></summary>
            LEDRemote = 0x30,
            /// <summary></summary>
            LEDForward = 0x31,
            /// <summary></summary>
            LEDReverse = 0x32,
            /// <summary></summary>
            LEDStop = 0x33,
            /// <summary></summary>
            LEDRewind = 0x34,
            /// <summary></summary>
            LEDFastForward = 0x35,
            /// <summary></summary>
            LEDPlay = 0x36,
            /// <summary></summary>
            LEDPause = 0x37,
            /// <summary></summary>
            LEDRecord = 0x38,
            /// <summary></summary>
            LEDError = 0x39,
            /// <summary></summary>
            LEDSelectedIndicator = 0x3A,
            /// <summary></summary>
            LEDInUseIndicator = 0x3B,
            /// <summary></summary>
            LEDMultiModeIndicator = 0x3C,
            /// <summary></summary>
            LEDIndicatorOn = 0x3D,
            /// <summary></summary>
            LEDIndicatorFlash = 0x3E,
            /// <summary></summary>
            LEDIndicatorSlowBlink = 0x3F,
            /// <summary></summary>
            LEDIndicatorFastBlink = 0x40,
            /// <summary></summary>
            LEDIndicatorOff = 0x41,
            /// <summary></summary>
            LEDFlashOnTime = 0x42,
            /// <summary></summary>
            LEDSlowBlinkOnTime = 0x43,
            /// <summary></summary>
            LEDSlowBlinkOffTime = 0x44,
            /// <summary></summary>
            LEDFastBlinkOnTime = 0x45,
            /// <summary></summary>
            LEDFastBlinkOffTime = 0x46,
            /// <summary></summary>
            LEDIndicatorColor = 0x47,
            /// <summary></summary>
            LEDRed = 0x48,
            /// <summary></summary>
            LEDGreen = 0x49,
            /// <summary></summary>
            LEDAmber = 0x4A,
            /// <summary></summary>
            LEDGenericIndicator = 0x3B,
            /// <summary></summary>
            TelephonyPhone = 0x01,
            /// <summary></summary>
            TelephonyAnsweringMachine = 0x02,
            /// <summary></summary>
            TelephonyMessageControls = 0x03,
            /// <summary></summary>
            TelephonyHandset = 0x04,
            /// <summary></summary>
            TelephonyHeadset = 0x05,
            /// <summary></summary>
            TelephonyKeypad = 0x06,
            /// <summary></summary>
            TelephonyProgrammableButton = 0x07,
            /// <summary></summary>
            SimulationRudder = 0xBA,
            /// <summary></summary>
            SimulationThrottle = 0xBB
        }

        /// <summary>Enumeration containing flags for a raw input device.</summary>
        [Flags()]
        public enum RawInputDeviceFlags : uint
        {
            /// <summary>No flags.</summary>
            None = 0,
            /// <summary>If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.</summary>
            Remove = 0x00000001,
            /// <summary>If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with PageOnly.</summary>
            Exclude = 0x00000010,
            /// <summary>If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that Usage must be zero. To exclude a particular top level collection, use Exclude.</summary>
            PageOnly = 0x00000020,
            /// <summary>If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages. This is only for the mouse and keyboard.</summary>
            NoLegacy = 0x00000030,
            /// <summary>If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that WindowHandle must be specified.</summary>
            InputSink = 0x00000100,
            /// <summary>If set, the mouse button click does not activate the other window.</summary>
            CaptureMouse = 0x00000200,
            /// <summary>If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is NULL.</summary>
            NoHotKeys = 0x00000200,
            /// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
            AppKeys = 0x00000400,
            /// <summary>
            /// If set, this enables the caller to receive WM_INPUT_DEVICE_CHANGE notifications for device arrival and device removal.
            /// </summary>
            DevNotify = 0x00002000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public RawInputType dwType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public RawInputType dwType;
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
        public struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
#if WIN64
            [FieldOffset(24)]
#else
            [FieldOffset(16)]
#endif
            public RAWMOUSE mouse;

#if WIN64
            [FieldOffset(24)]
#else
            [FieldOffset(16)]
#endif
            public RAWKEYBOARD keyboard;

#if WIN64
            [FieldOffset(24)]
#else
            [FieldOffset(16)]
#endif
            public RAWHID hid;
        }

        /// <summary>Value type for raw input devices.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            /// <summary>Top level collection Usage page for the raw input device.</summary>
            public HIDUsagePage UsagePage;
            /// <summary>Top level collection Usage for the raw input device. </summary>
            public HIDUsage Usage;
            /// <summary>Mode flag that specifies how to interpret the information provided by UsagePage and Usage.</summary>
            public RawInputDeviceFlags Flags;
            /// <summary>Handle to the target device. If NULL, it follows the keyboard focus.</summary>
            public IntPtr WindowHandle;
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterRawInputDevices([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint puiNumDevices, int cbSize);

        public static uint GetRawInputDeviceList(IList<RAWINPUTDEVICELIST> devices)
        {
            uint uNumDevices = 0;
            int structSize = Marshal.SizeOf<RAWINPUTDEVICELIST>();
            _ = GetRawInputDeviceList(IntPtr.Zero, ref uNumDevices, structSize);

            uint ret = 0;

            if (uNumDevices > 0)
            {
                IntPtr ptr = Marshal.AllocHGlobal(structSize * (int)uNumDevices);
                ret = GetRawInputDeviceList(ptr, ref uNumDevices, structSize);

                for (int i = 0; i < uNumDevices; i++)
                {
                    RAWINPUTDEVICELIST rd = Marshal.PtrToStructure<RAWINPUTDEVICELIST>(IntPtr.Add(ptr, structSize * i));
                    devices.Add(rd);
                }

                Marshal.FreeHGlobal(ptr);
            }

            return ret;
        }

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        static extern int GetRawInputDeviceInfo(IntPtr hDevice, DeviceInfoTypes uiCommand, IntPtr pData, ref uint pcbSize);

        public static string GetRawInputDeviceInterfaceName(IntPtr hDevice, uint maxChars = 260)
        {
            uint ptrSize = maxChars * 2;
            IntPtr ptr = Marshal.AllocHGlobal((int)ptrSize);

            string name = null;
            if (GetRawInputDeviceInfo(hDevice, DeviceInfoTypes.RIDI_DEVICENAME, ptr, ref ptrSize) > 0)
            {
                name = Marshal.PtrToStringUni(ptr);
            }

            Marshal.FreeHGlobal(ptr);

            return name;
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref int pcbSize, int cbSizeHeader);

        public static bool GetRawInputData(IntPtr hRawInput, ref RAWINPUT pData)
        {
            int iSize = 0;
            _ = GetRawInputData(hRawInput, RID_INPUT, IntPtr.Zero, ref iSize, Marshal.SizeOf<RAWINPUTHEADER>());
            if (iSize == 0)
                return false;

            IntPtr mem = Marshal.AllocHGlobal(iSize);
            if (mem == IntPtr.Zero)
                return false;

            if (GetRawInputData(hRawInput, RID_INPUT, mem, ref iSize, Marshal.SizeOf<RAWINPUTHEADER>()) != iSize)
            {
                Marshal.FreeHGlobal(mem);
                return false;
            }

            pData = Marshal.PtrToStructure<RAWINPUT>(mem);
            Marshal.FreeHGlobal(mem);
            return true;
        }
    }
}
