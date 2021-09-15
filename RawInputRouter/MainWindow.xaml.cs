using PInvoke;
using Redirector.Core;
using Redirector.Core.Windows;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace RawInputRouter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null;

        public IWin32Redirector InputManager { get; } = new RIRRedirector();

        public static readonly DependencyProperty IsInCaptureModeProperty = DependencyProperty.Register(
            "IsInCaptureMode",
            typeof(bool),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public bool IsInCaptureMode { get => (bool)GetValue(IsInCaptureModeProperty); set => SetValue(IsInCaptureModeProperty, value); }

        private CaptureWindow CaptureWindow { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Process process = Process.GetCurrentProcess();
            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();

            if (!RawInput.RegisterRawInputDevices( new[]
                {
                    // Keyboard
                    new RawInput.RAWINPUTDEVICE()
                    {
                        UsagePage = RawInput.HIDUsagePage.Generic,
                        Usage = RawInput.HIDUsage.Keyboard,
                        Flags = RawInput.RawInputDeviceFlags.InputSink |  RawInput.RawInputDeviceFlags.DevNotify,
                        WindowHandle = hWnd
                    }
                }, 1, Marshal.SizeOf(typeof(RawInput.RAWINPUTDEVICE))))
            {
                User32.MessageBox(hWnd, "FATAL ERROR! RegisterRawInputDevices failed.", null, 0);
                process.Kill();
                return;
            }

            if (!WinMsgIntercept.Install(hWnd))
            {
                User32.MessageBox(hWnd, "FATAL ERROR! RirInstall failed.", null, 0);
                process.Kill();
                return;
            }

            HwndSource source = HwndSource.FromHwnd(hWnd);
            source.AddHook(new HwndSourceHook(WndProc));

            Instance = this;

            Debug.WriteLine("MainWindow() init complete");
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //  do stuff
            switch (msg)
            {
                case (int)User32.WindowMessage.WM_INPUT_DEVICE_CHANGE: // WM_INPUT_DEVICE_CHANGE
                    Instance.InputManager.ProcessRawInputDeviceChangedMessage(wParam, lParam);
                    break;

                case (int)User32.WindowMessage.WM_INPUT: // WM_INPUT
                    if (Instance.IsInCaptureMode)
                    {
                        Instance.CaptureWindow?.ProcessRawInputMessage(wParam, lParam);
                        handled = true;
                        return new IntPtr(1);
                    }

                    if (Instance.InputManager.ProcessRawInputMessage(wParam, lParam))
                    {
                        handled = true;
                        return new IntPtr(1);
                    }
                    break;

                case WinMsgIntercept.WM_HOOK_KEYBOARD_INTERCEPT: // Keyboard Intercept hook
                    if (Instance.InputManager.ProcessKeyboardInterceptMessage(wParam, lParam))
                    {
                        handled = true;
                        return new IntPtr(1);
                    }
                        
                    break;

                case WinMsgIntercept.WM_HOOK_CBT: // Window activity hook
                    Instance.InputManager.ProcessWindowMessage(wParam, lParam);
                    break;

#if DEBUG
                case WinMsgIntercept.WM_DEBUG_OUTPUT: // Generic debug output
                    Debug.WriteLine("Debug output message: " + wParam + ", " + lParam );
                    break;
#endif
            }

            return IntPtr.Zero;
        }

        private void OnDevicePathChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private bool VerifyDeviceCapture(RIRDeviceSource tempDevice, RIRDeviceSource device, ref string s)
        {
            if (InputManager.Devices.FirstOrDefault(d2 => d2 != device && d2.Path.Equals(tempDevice.Path, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                s = "A registered device with the same path already exists.";
                return false;
            }
            return true;
        }

        private void ButtonCaptureNewDevice_Click(object sender, RoutedEventArgs e)
        {
            CaptureWindow = new CaptureWindow();
            CaptureWindow.SetBinding(CaptureWindow.IsCapturingProperty, new Binding("IsInCaptureMode")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });
            CaptureWindow.VerifyResult += VerifyDeviceCapture;
            CaptureWindow.AcceptResult += (td, d) =>
            {
                InputManager.Devices.Add(td);
            };
            CaptureWindow.Show();
        }

        private void EditDeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var device = (RIRDeviceSource)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (device == null)
                return;

            CaptureWindow = new CaptureWindow();

            CaptureWindow.Device = device;
            CaptureWindow.TemporaryDevice.Name = device.Name;
            CaptureWindow.TemporaryDevice.Path = device.Path;
            CaptureWindow.TemporaryDevice.Handle = device.Handle;

            CaptureWindow.SetBinding(CaptureWindow.IsCapturingProperty, new Binding("IsInCaptureMode")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });
            CaptureWindow.VerifyResult += VerifyDeviceCapture;
            CaptureWindow.AcceptResult += (td, d) =>
            {
                d.Name = td.Name;
                d.Path = td.Path;
                d.Handle = td.Handle;
            };
            CaptureWindow.IsDeviceVerified = true;
            CaptureWindow.Show();
        }

        private void DeleteDeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var device = (RIRDeviceSource)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (device == null)
                return;

            InputManager.Devices.Remove(device);
        }

        private bool VerifyProcessWindow(RIRApplicationReceiver tempWindow, RIRApplicationReceiver window, ref string s)
        {
            return true;
        }

        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProcessWindowDialog();

            window.Title = "Add New Application";
            window.VerifyResult += VerifyProcessWindow;
            window.AcceptResult += (tw, w) =>
            {
                InputManager.Applications.Add(tw);
            };

            window.Show();
        }

        private void EditProcessWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = (RIRApplicationReceiver)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (processWindow == null)
                return;

            var window = new ProcessWindowDialog();
            window.ProcessWindow = processWindow;
            window.TemporaryProcessWindow.Name = processWindow.Name;
            window.TemporaryProcessWindow.ExecutableName = processWindow.ExecutableName;
            window.TemporaryProcessWindow.WindowTitleSearch = processWindow.WindowTitleSearch;
            window.TemporaryProcessWindow.WindowTitleSearchMethod = processWindow.WindowTitleSearchMethod;
            window.TemporaryProcessWindow.FindWindow();

            window.Title = "Edit Application";
            window.VerifyResult += VerifyProcessWindow;
            window.AcceptResult += (tw, w) =>
            {
                w.Name = tw.Name;
                w.ExecutableName = tw.ExecutableName;
                w.WindowTitleSearch = tw.WindowTitleSearch;
                w.WindowTitleSearchMethod = tw.WindowTitleSearchMethod;
                w.FindWindow();
            };

            window.Show();
        }

        private void DeleteProcessWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = (RIRApplicationReceiver)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (processWindow == null)
                return;

            InputManager.Applications.Remove(processWindow);
        }

        private void IRDeviceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var items = new List<IDeviceSource>(InputManager.Devices);
            items.Insert(0, null);
            ((ComboBox)sender).ItemsSource = items;
        }

        private void IRProcessWindowComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var items = new List<IApplicationReceiver>(InputManager.Applications);
            items.Insert(0, null);
            ((ComboBox)sender).ItemsSource = items;
        }

        private void ButtonNewInputRoute_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen = true;
        }

        private void NewKeyboardIRMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputManager.Routes.Add(new Route()
            {
                Source = InputManager.Devices.FirstOrDefault(),
                Destination = InputManager.Applications.FirstOrDefault(),
                Triggers = {
                    new KeyboardRouteTrigger()
                    {
                        KeyState = KeyboardRouteInputKeyState.Down,
                        Key = Key.PageDown
                    } 
                },
                Actions =
                {
                    new AcceleratorOutputAction()
                    {
                        Accelerator = 393
                    }
                }
            });

            InputManager.Routes.Add(new Route()
            {
                Source = InputManager.Devices.FirstOrDefault(),
                Destination = InputManager.Applications.FirstOrDefault(),
                Triggers = {
                    new KeyboardRouteTrigger()
                    {
                        KeyState = KeyboardRouteInputKeyState.Down,
                        Key = Key.PageUp
                    } 
                },
                Actions =
                {
                    new AcceleratorOutputAction()
                    {
                        Accelerator = 394
                    }
                }
            });
        }

        private void DeleteInputRouteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var inputRoute = (IRoute)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (inputRoute == null)
                return;

            InputManager.Routes.Remove(inputRoute);
        }
    }
}
