using CommunityToolkit.Mvvm.ComponentModel;
using RawInputRouter.Imports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RawInputRouter.Routing
{
    public class RawInputInfo
    {
        public User32.RawInputType Type;
        public User32.RAWINPUTHEADER Header = new();
        public User32.RAWMOUSE Mouse = new();
        public User32.RAWKEYBOARD Keyboard = new();
        public User32.RAWHID HID = new();
    }

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
            WinMsgIntercept.CBT cbt = new();
            WinMsgIntercept.GetCBT(ref cbt);

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
            RawInputInfo info = new();
            User32.GetRawInputData(lParam, ref info.Header, ref info.Mouse, ref info.Keyboard, ref info.HID);
            info.Type = (User32.RawInputType)info.Header.dwType;

            DeviceInput input = null;

            switch (info.Type)
            {
                case User32.RawInputType.Keyboard:

                    // Correct the raw input mess
                    // https://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/

                    int scanCode = info.Keyboard.MakeCode;
                    User32.VK virtualKey = (User32.VK)info.Keyboard.VKey;
                    bool isE0 = (info.Keyboard.Flags & User32.RI_KEY_E0) != 0;
                    bool isE1 = (info.Keyboard.Flags & User32.RI_KEY_E1) != 0;

                    switch (virtualKey)
                    {
                        case (User32.VK)255:
                            return false; // Discard "fake keys"
                        case User32.VK.SHIFT:
                            // Correct left-hand / right-hand SHIFT
                            virtualKey = (User32.VK)User32.MapVirtualKey((uint)scanCode, (uint)virtualKey);
                            break;
                        case User32.VK.NUMLOCK:
                            // Correct PAUSE/BREAK and NUM LOCK silliness, and set the extended bit
                            scanCode = (int)User32.MapVirtualKey((uint)virtualKey, User32.MAPVK_VK_TO_VSC) | 0x100;
                            break;
                    }

                    if (isE1)
                    {
                        // For escaped sequences, turn the virtual key into the correct scan code using MapVirtualKey.
                        // However, MapVirtualKey is unable to map VK_PAUSE (this is a known bug), hence we map that by hand.
                        if (virtualKey == User32.VK.PAUSE)
                            scanCode = 0x45;
                        else
                            scanCode = (int)User32.MapVirtualKey((uint)virtualKey, User32.MAPVK_VK_TO_VSC);
                    }

                    input = new KeyboardDeviceInput()
                    {
                        DeviceHandle = info.Header.hDevice,
                        Time = tickCount,
                        VKey = (int)virtualKey,
                        ScanCode = scanCode,
                        Extended = isE0,
                        IsKeyDown = info.Keyboard.Message == User32.WM_KEYDOWN
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
            
            WinMsgIntercept.KeyboardInput keyboardInput = new();
            WinMsgIntercept.GetKeyboardInput(ref keyboardInput);

            // Can't directly cast from IntPtr -> int on 64-bit, otherwise we'll get an OverflowException.
            // Pain.
            int virtualKey = IntPtr.Size == 8 ? (int)(ulong)wParam : (int)wParam; 
            uint flags = IntPtr.Size == 8 ? (uint)(ulong)lParam : (uint)lParam;

            KeyboardDeviceInput input = new()
            {
                Time = tickCount,
                VKey = virtualKey,
                ScanCode = (int)((flags >> 16) & 0xFF),
                Extended = (flags & (1 << 24)) > 0,
                IsKeyDown = (flags & (0x1 << 31)) == 0
            };

            CleanInputBuffer();

            DeviceInput matchedInput = MatchBufferDeviceInput(input);

            // Handle edge case where holding doing key repeatedly might "leak"
            if (matchedInput == null && input.IsKeyDown)
            {
                // Try to see if any device is currently holding the key.
                IDeviceSource matchedSource = Devices.Where(d => d.State.GetVirtualKeyState(input))
                    .FirstOrDefault();

                if (matchedSource != null)
                {
                    // dumb but works
                    input.DeviceHandle = matchedSource.Handle;
                    matchedInput = input;
                }
            }

            if (matchedInput != null)
            {
                // Find device that made the input.
                IDeviceSource matchedSource = Devices.Where(d => d.Handle == matchedInput.DeviceHandle)
                    .FirstOrDefault();

                if (matchedSource != null)
                {
                    InputBuffer.Remove(matchedInput);
                }

                return ShouldBlockOriginalInput(matchedSource, matchedInput);
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
                    string deviceName = User32.GetRawInputDeviceName(lParam);
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
                if ((Environment.TickCount - input.Time) > 5000)
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

        protected virtual DeviceInput MatchBufferDeviceInput(DeviceInput bufferInput)
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
