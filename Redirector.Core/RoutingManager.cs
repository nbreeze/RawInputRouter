using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PInvoke;
using Redirector.Native;

namespace Redirector.Core
{
    public class RoutingManager : ObservableObject, IRoutingManager
    {
        public virtual ObservableCollection<IDeviceSource> Devices { get; } = new();

        public virtual ObservableCollection<IApplicationReceiver> Applications { get; } = new();

        public virtual ObservableCollection<IRoute> Routes { get; } = new();

        private List<DeviceInput> InputBuffer = new();

        public RoutingManager() : base()
        {
            Devices.CollectionChanged += OnDevicesCollectionChanged;
            Applications.CollectionChanged += OnWindowsCollectionChanged;
        }

        public virtual void ProcessWindowMessage(IntPtr wParam, IntPtr lParam)
        {
            WinMsgIntercept.GetCBT(out var cbt);

            IEnumerable<IApplicationReceiver> apps = null;

            switch (cbt.Code)
            {
                case 3: // HCBT_CREATEWND
                    foreach (var app in Applications)
                    {
                        if (User32.IsWindow(app.Handle))
                            continue;

                        app.FindWindow();
                    }

                    break;

                case 4: // HCBT_DESTROYWND
                    apps = Applications.Where(app => app.Handle == wParam);
                    foreach (var app in apps)
                    {
                        if (app != null)
                        {
                            app.Handle = IntPtr.Zero;
                            app.Process = null;
                        }
                    }

                    break;
            }
        }

        public virtual bool ProcessRawInputMessage(IntPtr wParam, IntPtr lParam)
        {
            int tickCount = Environment.TickCount;
            RawInput.RAWINPUT info = new();
            RawInput.GetRawInputData(lParam, ref info);

            DeviceInput input = null;

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
                            return false; // Discard "fake keys"
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

                    input = new KeyboardDeviceInput()
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
                IDeviceSource source = Devices.Where(d => d.Handle == input.DeviceHandle).FirstOrDefault();
                if (source != null)
                {
                    OnInput(source, input);
                }
            }

            return false;
        }

        public virtual bool ProcessKeyboardInterceptMessage(IntPtr wParam, IntPtr lParam)
        {
            int tickCount = Environment.TickCount;

            WinMsgIntercept.GetKeyboardInput(out var keyboardInput);

            // Can't directly cast from IntPtr -> int on 64-bit, otherwise we'll get an OverflowException.
            // Pain.
            int virtualKey = Environment.Is64BitProcess ? (int)(ulong)wParam : (int)wParam;
            uint flags = Environment.Is64BitProcess ? (uint)(ulong)lParam : (uint)lParam;

            KeyboardDeviceInput input = new()
            {
                Time = tickCount,
                VKey = virtualKey,
                ScanCode = (int)(flags >> 16 & 0xFF),
                Extended = (flags & 1 << 24) > 0,
                IsKeyDown = (flags & 1 << 31) == 0
            };

            CleanInputBuffer();

            DeviceInput matchedInput = MatchDeviceInputInBuffer(input);
            if (matchedInput != null)
            {
                InputBuffer.Remove(matchedInput);

                // Find device source that made the input. This should not be null.
                IDeviceSource source = Devices.Where(d => d.Handle == matchedInput.DeviceHandle)
                    .First();

                return ShouldBlockOriginalInput(source, matchedInput);
            }
            else
            {
                // Handle edge case where holding down key repeatedly might "leak"
                if (input.IsKeyDown)
                {
                    // Try to see if a device is currently holding the key.
                    IDeviceSource source = Devices.Where(d => d.State.GetVirtualKeyState(input))
                        .FirstOrDefault();

                    if (source != null)
                    {
                        input.DeviceHandle = source.Handle;
                        return ShouldBlockOriginalInput(source, input);
                    }
                }
            }

            return false;
        }

        public virtual void ProcessRawInputDeviceChangedMessage(IntPtr wParam, IntPtr lParam)
        {
            int msgType = wParam.ToInt32();
            IDeviceSource device;

            switch (msgType)
            {
                case 1: // GIDC_ARRIVAL
                    string deviceName = RawInput.GetRawInputDeviceInterfaceName(lParam);
                    device = Devices.Where(d => d.Path.Equals(deviceName)).FirstOrDefault();
                    if (device != null)
                    {
                        device.Handle = lParam;
                        device.OnConnect();
                    }

                    break;
                case 2: // GIDC_REMOVAL
                    device = Devices.Where(d => d.Handle == lParam).FirstOrDefault();
                    if (device != null)
                    {
                        device.OnDisconnect();
                        device.Handle = IntPtr.Zero;
                    }
                    break;
            }
        }

        public virtual void OnInput(IDeviceSource source, DeviceInput input)
        {
            InputBuffer.Add(input);

            source.OnInput(input);

            DispatchInputToRoutes(source, input);
        }

        protected void DispatchInputToRoutes(IDeviceSource source, DeviceInput input)
        {
            foreach (IRoute route in Routes)
            {
                route.OnInput(source, input);
            }
        }

        public virtual bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input)
        {
            if (source.ShouldBlockOriginalInput(input))
                return true;

            foreach (IRoute route in Routes)
            {
                if (route.ShouldBlockOriginalInput(source, input))
                    return true;
            }

            return false;
        }

        protected void CleanInputBuffer()
        {
            List<DeviceInput> removeInputs = new List<DeviceInput>();

            foreach (DeviceInput input in InputBuffer)
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

        protected virtual DeviceInput MatchDeviceInputInBuffer(DeviceInput bufferInput)
        {
            DeviceInput matchedInput = InputBuffer.SkipWhile(input => !input.Matches(bufferInput))
                .FirstOrDefault();

            return matchedInput;
        }

        protected virtual void OnWindowsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    ApplicationReceiver window = (ApplicationReceiver)item;

                    if (window == null)
                        continue;

                    window.OnDelete();

                    foreach (var route in Routes)
                    {
                        if (route.Destination == window)
                        {
                            route.Destination = null;
                        }
                    }
                }
            }
        }

        protected virtual void OnDevicesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    IDeviceSource source = item as IDeviceSource;
                    if (source == null)
                        continue;

                    source.OnDelete();

                    foreach (var route in Routes)
                    {
                        if (route.Source == source)
                        {
                            route.Source = null;
                        }
                    }
                }
            }
        }
    }
}
