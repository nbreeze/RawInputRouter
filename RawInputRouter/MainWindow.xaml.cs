using RawInputRouter.Imports;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RawInputRouter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null;

        public InputManager InputManager { get; } = new InputManager();

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

            if (!User32.RegisterRawInputDevices(new List<User32.RAWINPUTDEVICE>()
                {
                    // Keyboard
                    new User32.RAWINPUTDEVICE()
                    {
                        usUsagePage = 0x1,
                        usUsage = 0x6,
                        dwFlags = 0x100 | 0x2000, // RIDEV_INPUTSINK | RIDEV_DEVNOTIFY
                        hwndTarget = hWnd
                    }
                }))
            {
                User32.MessageBox(hWnd, "FATAL ERROR! RegisterRawInputDevices failed.", null, 0);
                process.Kill();
                return;
            }

            if (!RirListener.Install(hWnd))
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
                case 0xfe: // WM_INPUT_DEVICE_CHANGE
                    return Instance.InputManager.OnRawInputDeviceChangedMessage(wParam, lParam, ref handled);

                case 0xff: // WM_INPUT
                    if (Instance.IsInCaptureMode)
                        Instance.CaptureWindow?.OnRawInputMessage(wParam, lParam);

                    return Instance.InputManager.OnRawInputMessage(wParam, lParam, ref handled);

                case 0xdabb: // Window activity hook
                    Instance.InputManager.OnCBTMessage(wParam, lParam);
                    break;

                case 0xface: // Keyboard Intercept hook
                    return Instance.InputManager.OnInputInterceptMessage(1, wParam, lParam, ref handled);
            }

            return IntPtr.Zero;
        }

        private void OnDevicePathChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private bool VerifyDeviceCapture(InputManager.Device tempDevice, InputManager.Device device, ref string s)
        {
            if (InputManager.Devices.FirstOrDefault(d2 => d2 != device && d2.DevicePath.Equals(tempDevice.DevicePath, StringComparison.InvariantCultureIgnoreCase)) != null)
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
            var device = (InputManager.Device)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (device == null)
                return;

            CaptureWindow = new CaptureWindow();

            CaptureWindow.Device = device;
            CaptureWindow.TemporaryDevice.Name = device.Name;
            CaptureWindow.TemporaryDevice.DevicePath = device.DevicePath;
            CaptureWindow.TemporaryDevice.DeviceHandle = device.DeviceHandle;

            CaptureWindow.SetBinding(CaptureWindow.IsCapturingProperty, new Binding("IsInCaptureMode")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });
            CaptureWindow.VerifyResult += VerifyDeviceCapture;
            CaptureWindow.AcceptResult += (td, d) =>
            {
                d.Name = td.Name;
                d.DevicePath = td.DevicePath;
                d.DeviceHandle = td.DeviceHandle;
            };
            CaptureWindow.IsDeviceVerified = true;
            CaptureWindow.Show();
        }

        private void DeleteDeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var device = (InputManager.Device)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (device == null)
                return;

            InputManager.Devices.Remove(device);
        }

        private bool VerifyProcessWindow(InputManager.ProcessWindow tempWindow, InputManager.ProcessWindow window, ref string s)
        {
            return true;
        }

        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProcessWindowDialog();

            window.Title = "Add New Process Window";
            window.VerifyResult += VerifyProcessWindow;
            window.AcceptResult += (tw, w) =>
            {
                tw.Update(true);
                InputManager.ProcessWindows.Add(tw);
            };

            window.Show();
        }

        private void EditProcessWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = (InputManager.ProcessWindow)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (processWindow == null)
                return;

            var window = new ProcessWindowDialog();
            window.ProcessWindow = processWindow;
            window.TemporaryProcessWindow.Name = processWindow.Name;
            window.TemporaryProcessWindow.ExeName = processWindow.ExeName;
            window.TemporaryProcessWindow.WindowTitleSearch = processWindow.WindowTitleSearch;
            window.TemporaryProcessWindow.WindowTitleSearchMethod = processWindow.WindowTitleSearchMethod;
            window.TemporaryProcessWindow.Update();

            window.Title = "Edit Process Window";
            window.VerifyResult += VerifyProcessWindow;
            window.AcceptResult += (tw, w) =>
            {
                w.Name = tw.Name;
                w.ExeName = tw.ExeName;
                w.WindowTitleSearch = tw.WindowTitleSearch;
                w.WindowTitleSearchMethod = tw.WindowTitleSearchMethod;
                w.Update(true);
            };

            window.Show();
        }

        private void DeleteProcessWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = (InputManager.ProcessWindow)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (processWindow == null)
                return;

            InputManager.ProcessWindows.Remove(processWindow);
        }

        private void IRDeviceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var items = new List<InputManager.Device>(InputManager.Devices);
            items.Insert(0, null);
            ((ComboBox)sender).ItemsSource = items;
        }

        private void IRProcessWindowComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var items = new List<InputManager.ProcessWindow>(InputManager.ProcessWindows);
            items.Insert(0, null);
            ((ComboBox)sender).ItemsSource = items;
        }

        private void ButtonNewInputRoute_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen = true;
        }

        private void NewKeyboardIRMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputManager.InputRoutes.Add(new InputManager.KeyboardInputRoute());
        }

        private void DeleteInputRouteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var inputRoute = (InputManager.InputRoute)((ListViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Content;
            if (inputRoute == null)
                return;

            InputManager.InputRoutes.Remove(inputRoute);
        }
    }
}
