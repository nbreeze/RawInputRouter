using PInvoke;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redirector.Core.Windows
{
    public class Win32Redirector : Redirector, IWin32Redirector
    {
        /// <summary>
        /// Stores the inputs retrieved via raw input.
        /// 
        /// <para>
        /// WM_INPUT messages contain raw input data, along with a Handle of the
        /// device that sent the input. However, keyboard hooks such as
        /// WH_KEYBOARD and WH_KEYBOARD_LL do not contain information about the
        /// device that sent the input, and those hooks are the only hooks that
        /// can reliably intercept and block input.
        /// </para>
        /// 
        /// <para>
        /// In order to block input on a per device basis, the input gained from
        /// those hooks have to be matched with an input stored in the input
        /// buffer. If the input is matched, then we can somewhat determine the
        /// device the input can from.
        /// </para>
        /// </summary>
        protected List<Win32DeviceInput> InputBuffer = new();

        public override void OnInput(IDeviceSource source, DeviceInput input)
        {
            if (input is Win32DeviceInput _input)
            {
                InputBuffer.Add(_input);
            }

            base.OnInput(source, input);
        }

        protected void CleanInputBuffer()
        {
            List<DeviceInput> removeInputs = new List<DeviceInput>();

            foreach (Win32DeviceInput input in InputBuffer)
            {
                if (Environment.TickCount - input.Time > 5000)
                {
                    removeInputs.Add(input);
                }
                else
                {
                    break;
                }
            }

            InputBuffer.RemoveAll(input => removeInputs.Contains(input));
        }

        protected virtual Win32DeviceInput MatchDeviceInputInBuffer(Win32DeviceInput bufferInput)
        {
            Win32DeviceInput matchedInput = InputBuffer.SkipWhile(input => !input.Matches(bufferInput))
                .FirstOrDefault();

            return matchedInput;
        }

        public virtual void ProcessWindowMessage(IntPtr wParam, IntPtr lParam)
        {
            WinMsgIntercept.GetCBT(out var cbt);

            IEnumerable<IApplicationReceiver> apps = null;

            switch (cbt.Code)
            {
                case 3: // HCBT_CREATEWND
                    foreach (IApplicationReceiver _app in Applications)
                    {
                        if (_app is not IWin32ApplicationReceiver app)
                            continue;

                        if (User32.IsWindow(app.Handle))
                            continue;

                        app.FindWindow();
                    }

                    break;

                case 4: // HCBT_DESTROYWND
                    apps = Applications.Where(_app => _app is IWin32ApplicationReceiver app && app.Handle == wParam);
                    foreach (IApplicationReceiver _app in apps)
                    {
                        if (_app is not IWin32ApplicationReceiver app)
                            continue;

                        app.Handle = IntPtr.Zero;
                        app.ProcessId = 0;
                    }

                    break;
            }
        }

        /// <summary>
        /// Process a WM_INPUT message.
        /// </summary>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        public virtual void ProcessRawInputMessage(IntPtr wParam, IntPtr lParam)
        {
            int tickCount = Environment.TickCount;
            RawInput.RAWINPUT info = new();
            RawInput.GetRawInputData(lParam, ref info);

            Win32DeviceInput input = null;

            switch (info.header.dwType)
            {
                case RawInput.RawInputType.Keyboard:

                    // Correct the raw input mess
                    // https://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/

                    int scanCode = info.keyboard.MakeCode;
                    User32.VirtualKey virtualKey = (User32.VirtualKey)info.keyboard.VKey;
                    bool isE0 = (info.keyboard.Flags & RawInput.RI_KEY_E0) != 0;
                    bool isE1 = (info.keyboard.Flags & RawInput.RI_KEY_E1) != 0;

                    switch (virtualKey)
                    {
                        case (User32.VirtualKey)255:
                            return; // Discard "fake keys"
                        case User32.VirtualKey.VK_SHIFT:
                            // Correct left-hand / right-hand SHIFT
                            virtualKey = (User32.VirtualKey)User32.MapVirtualKey(scanCode, (User32.MapVirtualKeyTranslation)virtualKey);
                            break;
                        case User32.VirtualKey.VK_NUMLOCK:
                            // Correct PAUSE/BREAK and NUM LOCK silliness, and set the extended bit
                            scanCode = User32.MapVirtualKey((int)virtualKey, User32.MapVirtualKeyTranslation.MAPVK_VK_TO_VSC) | 0x100;
                            break;
                    }

                    if (isE1)
                    {
                        // For escaped sequences, turn the virtual key into the correct scan code using MapVirtualKey.
                        // However, MapVirtualKey is unable to map VK_PAUSE (this is a known bug), hence we map that by hand.
                        if (virtualKey == User32.VirtualKey.VK_PAUSE)
                            scanCode = 0x45;
                        else
                            scanCode = User32.MapVirtualKey((int)virtualKey, User32.MapVirtualKeyTranslation.MAPVK_VK_TO_VSC);
                    }

                    input = new Win32KeyboardDeviceInput()
                    {
                        DeviceHandle = info.header.hDevice,
                        Time = tickCount,
                        VKey = (int)virtualKey,
                        ScanCode = scanCode,
                        Extended = isE0,
                        IsKeyDown = info.keyboard.Message == (int)User32.WindowMessage.WM_KEYDOWN
                    };
                    break;
            }

            if (input != null)
            {
                IDeviceSource source = Devices.Where(_d => _d is IWin32DeviceSource d && d.Handle == input.DeviceHandle).FirstOrDefault();
                OnInput(source, input);
            }
        }

        public virtual bool ProcessKeyboardInterceptMessage(IntPtr wParam, IntPtr lParam)
        {
            int tickCount = Environment.TickCount;

            WinMsgIntercept.GetKeyboardInput(out var keyboardInput);

            // Can't directly cast from IntPtr -> int on 64-bit, otherwise we'll get an OverflowException.
            // Pain.
            int virtualKey = Environment.Is64BitProcess ? (int)(ulong)wParam : (int)wParam;
            uint flags = Environment.Is64BitProcess ? (uint)(ulong)lParam : (uint)lParam;

            Win32KeyboardDeviceInput input = new()
            {
                Time = tickCount,
                VKey = virtualKey,
                ScanCode = (int)(flags >> 16 & 0xFF),
                Extended = (flags & 1 << 24) > 0,
                IsKeyDown = (flags & 1 << 31) == 0
            };

            CleanInputBuffer();

            Win32DeviceInput matchedInput = MatchDeviceInputInBuffer(input);
            if (matchedInput != null)
            {
                InputBuffer.Remove(matchedInput);

                // Find device source that made the input. This should not be null.
                IDeviceSource source = Devices.Where(_d => _d is IWin32DeviceSource d && d.Handle == matchedInput.DeviceHandle)
                    .FirstOrDefault();

                return ShouldBlockOriginalInput(source, matchedInput);
            }
            else
            {
                // Handle edge case where holding down key repeatedly might "leak"
                if (input.IsKeyDown)
                {
                    // Try to see if a device is currently holding the key.
                    IDeviceSource source = Devices.Where(_d => _d is IWin32DeviceSource d && d.VirtualKeyStates.GetVirtualKeyState(input))
                        .FirstOrDefault();

                    if (source != null)
                    {
                        input.DeviceHandle = (source as IWin32DeviceSource).Handle;
                        return ShouldBlockOriginalInput(source, input);
                    }
                }
            }

            return false;
        }

        public virtual void ProcessRawInputDeviceChangedMessage(IntPtr wParam, IntPtr lParam)
        {
            int msgType = wParam.ToInt32();
            IWin32DeviceSource device;

            switch (msgType)
            {
                case 1: // GIDC_ARRIVAL
                    string deviceName = RawInput.GetRawInputDeviceInterfaceName(lParam);
                    device = Devices.Where(_d => _d is IWin32DeviceSource d && d.Path.Equals(deviceName)).FirstOrDefault() as IWin32DeviceSource;
                    if (device != null)
                    {
                        device.Handle = lParam;
                        device.OnConnect();
                    }

                    break;
                case 2: // GIDC_REMOVAL
                    device = Devices.Where(_d => _d is IWin32DeviceSource d && d.Handle == lParam).FirstOrDefault() as IWin32DeviceSource;
                    if (device != null)
                    {
                        device.OnDisconnect();
                        device.Handle = IntPtr.Zero;
                    }
                    break;
            }
        }

        public virtual void OnWindowTextChanged(IntPtr hWnd)
        {
            foreach (IApplicationReceiver _app in Applications)
            {
                if (_app is not IWin32ApplicationReceiver app)
                    continue;

                if (User32.IsWindow(app.Handle))
                    continue;

                app.FindWindow();
            }
        }
    }
}
