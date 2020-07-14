using RawInputRouter.Imports;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RawInputRouter
{
    public enum ProcessWindowTitleSearch
    {
        Exact,
        Match,
        Regex
    }

    public class ProcessWindowTitleSearchEnumConverter : IValueConverter
    {
        internal static Dictionary<ProcessWindowTitleSearch, string> ProcessWindowTitleSearchNames = new Dictionary<ProcessWindowTitleSearch, string>()
            {
                { ProcessWindowTitleSearch.Exact, "Exact" },
                { ProcessWindowTitleSearch.Match, "Match" },
                { ProcessWindowTitleSearch.Regex, "Regex" }
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = "";
            ProcessWindowTitleSearchNames.TryGetValue((ProcessWindowTitleSearch)value, out name);
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InputManager : NotifyPropertyChangedImpl
    {
        public class Device : NotifyPropertyChangedImpl
        {
            private string _Name = "";
            public string Name { get => _Name; set => SetPropertyValue(ref _Name, value); }

            private string _DevicePath = "";
            public string DevicePath { get => _DevicePath; set => SetPropertyValue(ref _DevicePath, value); }

            private bool _BlockInput = false;
            public bool BlockInput { get => _BlockInput; set => SetPropertyValue(ref _BlockInput, value); }

            private IntPtr _DeviceHandle = IntPtr.Zero;
            public IntPtr DeviceHandle { get => _DeviceHandle; set => SetPropertyValue(ref _DeviceHandle, value); }

            public void Update()
            {
                if (DeviceHandle != IntPtr.Zero || string.IsNullOrWhiteSpace(DevicePath))
                    return;

                List<User32.RAWINPUTDEVICELIST> devices = new List<User32.RAWINPUTDEVICELIST>();
                User32.GetRawInputDeviceList(devices);

                foreach (User32.RAWINPUTDEVICELIST device in devices)
                {
                    string deviceName = User32.GetRawInputDeviceName(device.hDevice);
                    if (deviceName.Equals(DevicePath))
                    {
                        DeviceHandle = device.hDevice;
                        break;
                    }
                }
            }
        }

        public class ProcessWindow : NotifyPropertyChangedImpl
        {
            private string _Name = "";
            public string Name { get => _Name; set => SetPropertyValue(ref _Name, value); }

            private string _ExeName;
            public string ExeName { get => _ExeName; set => SetPropertyValue(ref _ExeName, value); }

            private string _WindowTitleSearch;
            public string WindowTitleSearch { get => _WindowTitleSearch; set => SetPropertyValue(ref _WindowTitleSearch, value); }

            private ProcessWindowTitleSearch _WindowTitleSearchMethod;
            public ProcessWindowTitleSearch WindowTitleSearchMethod { get => _WindowTitleSearchMethod; set => SetPropertyValue(ref _WindowTitleSearchMethod, value); }

            private string _WindowTitle;
            public string WindowTitle{ get => _WindowTitle; set => SetPropertyValue(ref _WindowTitle, value); }

            private IntPtr _WindowHandle;
            public IntPtr WindowHandle { get => _WindowHandle; set => SetPropertyValue(ref _WindowHandle, value); }

            public void Update(bool bForce=false)
            {
                if (!bForce && User32.IsWindow(WindowHandle))
                {
                    StringBuilder windowTitle = new StringBuilder(260);
                    User32.GetWindowText(WindowHandle, windowTitle, 260);

                    WindowTitle = windowTitle.ToString();

                    return;
                }

                if (!string.IsNullOrEmpty(ExeName))
                {
                    List<IntPtr> windows = new List<IntPtr>();
                    User32.EnumWindows(hWnd => {
                        if (User32.IsWindow(hWnd) && User32.IsWindowVisible(hWnd))
                        {
                            windows.Add(hWnd);
                        }

                        return true;
                    });

                    var targetProcesses = Process.GetProcessesByName(ExeName);
                    foreach (var targetProcess in targetProcesses)
                    {
                        RegexOptions regexFlags = RegexOptions.None;
                        Regex rx = null;
                        if (WindowTitleSearchMethod == ProcessWindowTitleSearch.Regex)
                        {
                            rx = new Regex(WindowTitleSearch, regexFlags);
                        }

                        foreach (IntPtr hWnd in windows)
                        {
                            int processId = 0;
                            User32.GetWindowThreadProcessId(hWnd, ref processId);

                            if (processId != targetProcess.Id)
                                continue;

                            StringBuilder windowTitle = new StringBuilder(260);
                            User32.GetWindowText(hWnd, windowTitle, 260);

                            bool bMatches = false;
                            if (string.IsNullOrEmpty(WindowTitleSearch))
                            {
                                bMatches = true;
                            }
                            else
                            {
                                switch (WindowTitleSearchMethod)
                                {
                                    case ProcessWindowTitleSearch.Exact:
                                        bMatches = windowTitle.ToString().Equals(WindowTitleSearch);
                                        break;
                                    case ProcessWindowTitleSearch.Match:
                                        bMatches = windowTitle.ToString().Contains(WindowTitleSearch);
                                        break;
                                    case ProcessWindowTitleSearch.Regex:
                                        bMatches = rx.IsMatch(windowTitle.ToString());
                                        break;
                                }
                            }

                            if (bMatches)
                            {
                                WindowTitle = windowTitle.ToString();
                                WindowHandle = hWnd;
                                return;
                            }
                        }
                    }
                }

                WindowTitle = "";
                WindowHandle = IntPtr.Zero;
            }
        
            public IntPtr EnsureHandle()
            {
                if (WindowHandle == IntPtr.Zero)
                    Update();
                return WindowHandle;
            }
        }

        public abstract class InputRoute : NotifyPropertyChangedImpl
        {
            private Device _InputDevice;
            public Device InputDevice { get => _InputDevice; set => SetPropertyValue(ref _InputDevice, value); }

            private ProcessWindow _TargetWindow;
            public ProcessWindow TargetWindow { get => _TargetWindow; set => SetPropertyValue(ref _TargetWindow, value); }

            private bool _Enabled;
            public bool Enabled { get => _Enabled; set => SetPropertyValue(ref _Enabled, value); }

            private bool _CaptureAllInput;
            public bool CaptureAllInput { get => _CaptureAllInput; set => SetPropertyValue(ref _CaptureAllInput, value); }

            private bool _IgnoreIfFocused;
            public bool IgnoreIfFocused { get => _IgnoreIfFocused; set => SetPropertyValue(ref _IgnoreIfFocused, value); }

            public abstract User32.RawInputType Type { get; }
        }

        public class KeyboardInputRoute : InputRoute
        {
            public override User32.RawInputType Type => User32.RawInputType.Keyboard;

            private uint _InputKey;
            public uint InputKey { get => _InputKey; set => SetPropertyValue(ref _InputKey, value); }

            private uint _OutputKey = 0;
            public uint OutputKey { get => _OutputKey; set => SetPropertyValue(ref _OutputKey, value); }
        }

        internal abstract class BufferedInput
        {
            public IntPtr DeviceHandle = IntPtr.Zero;
            public int Time = 0;
        }

        internal sealed class BufferedKeyboardInput : BufferedInput
        {
            public uint VKey = 0;
            public int Message = 0;
            public uint Flags = 0;
        }

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        public ObservableCollection<ProcessWindow> ProcessWindows { get; } = new ObservableCollection<ProcessWindow>();

        public ObservableCollection<InputRoute> InputRoutes { get; } = new ObservableCollection<InputRoute>();

        private ArrayList InputBuffer = new ArrayList();

        private Timer ProcessWindowUpdateTimer;
        private Stopwatch ProcessWindowUpdateTimerWatch;
        private bool ProcessWindowUpdateTimerFastTracked = false;

        public InputManager() : base()
        {
            Devices.CollectionChanged += OnDevicesCollectionChanged;
            ProcessWindows.CollectionChanged += OnProcessWindowsCollectionChanged;

            ProcessWindowUpdateTimer = new Timer(5000);
            ProcessWindowUpdateTimerWatch = new Stopwatch();
            ProcessWindowUpdateTimer.Elapsed += (o, e) =>
            {
                UpdateProcessWindows();
                ProcessWindowUpdateTimerWatch.Restart();
                ((Timer)o).Interval = 5000;
                ((Timer)o).Start();
                ProcessWindowUpdateTimerFastTracked = false;
            };
            ProcessWindowUpdateTimer.AutoReset = false;
            ProcessWindowUpdateTimer.Start();
            ProcessWindowUpdateTimerWatch.Start();
        }

        private void OnProcessWindowsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var processWindow in e.OldItems)
                {
                    if (processWindow == null)
                        continue;

                    var inputRoutes = InputRoutes.Where(inputRoute => inputRoute.TargetWindow == processWindow);
                    foreach (var inputRoute in inputRoutes)
                    {
                        inputRoute.TargetWindow = null;
                    }
                }
            }
        }

        private void OnDevicesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove )
            {
                foreach (var device in e.OldItems)
                {
                    if (device == null)
                        continue;

                    var inputRoutes = InputRoutes.Where(inputRoute => inputRoute.InputDevice == device);
                    foreach( var inputRoute in inputRoutes )
                    {
                        inputRoute.InputDevice = null;
                    }
                }
            }
        }

        private void CleanInputBuffer()
        {
            while (InputBuffer.Count > 0)
            {
                var input = (BufferedInput)InputBuffer[0];
                if (Environment.TickCount - input.Time > 5000)
                {
                    InputBuffer.RemoveAt(0);
                    continue;
                }

                break;
            }
        }

        public Device GetRegisteredDevice(string devicePath)
        {
            return Devices.FirstOrDefault(d => d.DevicePath.Equals(devicePath, StringComparison.InvariantCultureIgnoreCase));
        }

        public Device GetRegisteredDevice(IntPtr deviceHandle)
        {
            return Devices.FirstOrDefault(d => d.DeviceHandle == deviceHandle);
        }

        public IEnumerable<ProcessWindow> GetRegisteredProcessWindows(IntPtr windowHandle)
        {
            return ProcessWindows.Where(w => w.WindowHandle == windowHandle);
        }

        public void UpdateProcessWindows(bool force=false)
        {
            foreach (var processWindow in ProcessWindows)
            {
                processWindow.Update(force);
            }
        }

        public IntPtr OnRawInputDeviceChangedMessage(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int msgType = wParam.ToInt32();
            Device device;

            switch (msgType)
            {
                case 1: // GIDC_ARRIVAL
                    device = GetRegisteredDevice(User32.GetRawInputDeviceName(lParam));
                    if (device != null)
                    {
                        device.DeviceHandle = lParam;
                    }

                    break;
                case 2: // GIDC_REMOVAL
                    device = GetRegisteredDevice(lParam);
                    if (device != null)
                    {
                        device.DeviceHandle = IntPtr.Zero;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        public IntPtr OnRawInputMessage(IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var tickCount = Environment.TickCount;
            var header = new User32.RAWINPUTHEADER();
            var mouse = new User32.RAWMOUSE();
            var keyboard = new User32.RAWKEYBOARD();
            var hid = new User32.RAWHID();

            User32.GetRawInputData(lParam, ref header, ref mouse, ref keyboard, ref hid);

            switch ((User32.RawInputType)header.dwType)
            {
                case User32.RawInputType.Keyboard:
                    InputBuffer.Add(new BufferedKeyboardInput()
                    {
                        DeviceHandle = header.hDevice,
                        Time = tickCount,
                        VKey = keyboard.VKey,
                        Message = keyboard.Message,
                        Flags = keyboard.Flags
                    });
                    break;
            }

            return IntPtr.Zero;
        }

        public void OnCBTMessage(IntPtr wParam, IntPtr lParam)
        {
            int code = 0;
            uint processId = 0;
            RirListener.GetCBT(ref code, ref processId);

            switch ( code )
            {
                case 3: // HCBT_CREATEWND
                    // We need to wait a little bit before checking the window.
                    if (!ProcessWindowUpdateTimerFastTracked && ProcessWindowUpdateTimerWatch.ElapsedMilliseconds < 4500)
                    {
                        ProcessWindowUpdateTimerFastTracked = true;
                        ProcessWindowUpdateTimer.Stop();
                        ProcessWindowUpdateTimer.Interval = 500;
                        ProcessWindowUpdateTimer.Start();
                    }

                    break;

                case 4: // HCBT_DESTROYWND
                    var windows = GetRegisteredProcessWindows(wParam);
                    foreach (var window in windows)
                    {
                        window.WindowTitle = "";
                        window.WindowHandle = IntPtr.Zero;
                    }

                    break;
            }
        }

        public IntPtr OnInputInterceptMessage(int inputType, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            List<InputRoute> handledInputRoutes = null;

            switch ((User32.RawInputType)inputType)
            {
                case User32.RawInputType.Keyboard: // RID_TYPEKEYBOARD
                    uint vk = 0;
                    uint processId = 0;
                    IntPtr wnd = IntPtr.Zero;
                    bool peek = false;

                    RirListener.GetKeyboardInput(ref vk, ref processId, ref wnd, ref peek);

                    CleanInputBuffer();

                    BufferedKeyboardInput matchedInput = null;
                    int inputIndex = -1;
                    for (int i = 0; i < InputBuffer.Count; i++)
                    {
                        if (!(InputBuffer[i] is BufferedKeyboardInput))
                            continue;

                        var input = (BufferedKeyboardInput)InputBuffer[i];
                        if (input.VKey == vk)
                        {
                            inputIndex = i;
                            matchedInput = input;
                            break;
                        }
                    }

                    if (matchedInput != null)
                    {
                        InputBuffer.RemoveAt(inputIndex);

                        // TODO: In some cases, the device handle is NULL, usually happens with precision touchpads (forwarded
                        // by a kernel driver). Handle it?

                        if (matchedInput.DeviceHandle != IntPtr.Zero)
                        {
                            var device = GetRegisteredDevice(matchedInput.DeviceHandle);
                            if (device != null)
                            {
                                var inputRoutes = InputRoutes.Where(ir => ir.Type == User32.RawInputType.Keyboard && ir.InputDevice == device);
                                foreach (KeyboardInputRoute inputRoute in inputRoutes)
                                {
                                    if (!inputRoute.CaptureAllInput && inputRoute.InputKey != matchedInput.VKey)
                                        continue;

                                    if (inputRoute.TargetWindow == null)
                                        continue;

                                    var windowHandle = inputRoute.TargetWindow.EnsureHandle();
                                    if (windowHandle == IntPtr.Zero)
                                        continue;

                                    if (inputRoute.IgnoreIfFocused)
                                    {
                                        var windowThreadId = User32.GetWindowThreadProcessId(windowHandle, IntPtr.Zero);
                                        var windowThreadInfo = new User32.GUITTHREADINFO();
                                        if (User32.GetGUIThreadInfo(windowThreadId, ref windowThreadInfo) && windowThreadInfo.hwndActive == windowHandle)
                                        {
                                            continue;
                                        }
                                    }
                                    
                                    if (inputRoute.CaptureAllInput)
                                    {
                                        User32.PostMessage(windowHandle, matchedInput.Message, new IntPtr(matchedInput.VKey), IntPtr.Zero);
                                    }
                                    else
                                    {
                                        User32.PostMessage(windowHandle, matchedInput.Message, new IntPtr(inputRoute.OutputKey == 0 ? matchedInput.VKey : inputRoute.OutputKey), IntPtr.Zero);
                                    }

                                    handledInputRoutes = handledInputRoutes ?? new List<InputRoute>();
                                    handledInputRoutes.Add(inputRoute);
                                }

                                if (device.BlockInput)
                                {
                                    handled = true;
                                    return new IntPtr(1);
                                }
                            }
                        }
                    }

                    break;
            }

            return IntPtr.Zero;
        }
    }

    public class InputRouteDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MouseTemplate { get; set; } = null;
        public DataTemplate KeyboardTemplate { get; set; } = null;
        public DataTemplate HidTemplate { get; set; } = null;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element == null)
                return null;

            var inputRoute = item as InputManager.InputRoute;
            if (inputRoute == null)
                return null;

            switch (inputRoute.Type)
            {
                case User32.RawInputType.Mouse:
                    return MouseTemplate;
                case User32.RawInputType.Keyboard:
                    return KeyboardTemplate;
                case User32.RawInputType.HumanInterfaceDevice:
                    return HidTemplate;
            }

            return null;
        }
    }
}
