using RawInputRouter.Imports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RawInputRouter
{
    /// <summary>
    /// Interaction logic for CaptureWindow.xaml
    /// </summary>
    public partial class CaptureWindow : Window
    {
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(
            "Device",
            typeof(InputManager.Device),
            typeof(CaptureWindow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty IsCapturingProperty = DependencyProperty.Register(
            "IsCapturing",
            typeof(bool),
            typeof(CaptureWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty IsDeviceVerifiedProperty = DependencyProperty.Register(
            "IsDeviceVerified",
            typeof(bool),
            typeof(CaptureWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(
            "ErrorText",
            typeof(string),
            typeof(CaptureWindow),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty TemporaryDeviceProperty = DependencyProperty.Register(
            "TemporaryDevice",
            typeof(InputManager.Device),
            typeof(CaptureWindow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public delegate bool VerifyResultDelegate(InputManager.Device tempDevice, InputManager.Device device, ref string errorText);
        public event VerifyResultDelegate VerifyResult;

        public delegate void AcceptResultDelegate(InputManager.Device tempDevice, InputManager.Device device);
        public event AcceptResultDelegate AcceptResult;

        public InputManager.Device Device { get => (InputManager.Device)GetValue(DeviceProperty); set => SetValue(DeviceProperty, value); }

        public InputManager.Device TemporaryDevice { get => (InputManager.Device)GetValue(TemporaryDeviceProperty); set => SetValue(TemporaryDeviceProperty, value); }

        public bool IsDeviceVerified { get => (bool)GetValue(IsDeviceVerifiedProperty); set => SetValue(IsDeviceVerifiedProperty, value); }

        public string ErrorText { get => (string)GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }

        public bool IsCapturing { get => (bool)GetValue(IsCapturingProperty); set => SetValue(IsCapturingProperty, value); }
        public bool IsClosing { get; private set; }

        public CaptureWindow()
        {
            InitializeComponent();

            TemporaryDevice = new InputManager.Device();

            Deactivated += CaptureWindow_Deactivated;
        }

        private void CaptureWindow_Deactivated(object sender, EventArgs e)
        {
            if (!IsClosing)
            {
                Close();
            }
        }

        public void StartCapturing()
        {
            if (IsCapturing)
                return;

            IsCapturing = true;
            IsDeviceVerified = false;
            ErrorText = "";
        }

        public void StopCapturing()
        {
            if (!IsCapturing)
                return;

            IsCapturing = false;
        }

        public void OnRawInputMessage(IntPtr wParam, IntPtr lParam)
        {
            var header = new User32.RAWINPUTHEADER();
            var mouse = new User32.RAWMOUSE();
            var keyboard = new User32.RAWKEYBOARD();
            var hid = new User32.RAWHID();

            User32.GetRawInputData(lParam, ref header, ref mouse, ref keyboard, ref hid);

            var hDevice = header.hDevice;
            if (hDevice != IntPtr.Zero)
            {
                StopCapturing();

                TemporaryDevice.DeviceHandle = hDevice;
                TemporaryDevice.DevicePath = User32.GetRawInputDeviceName(hDevice);

                var errorText = "";
                if (VerifyResult == null || VerifyResult(TemporaryDevice, Device, ref errorText))
                {
                    IsDeviceVerified = true;
                }
                else
                {
                    ErrorText = errorText;
                    IsDeviceVerified = false;
                }
            }
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCapturing)
                StartCapturing();
            else
                StopCapturing();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceVerified)
            {
                var name = TemporaryDevice.Name.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    ErrorText = "Name cannot be blank.";
                    return;
                }
            }

            ErrorText = "";

            AcceptResult?.Invoke(TemporaryDevice, Device);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            IsClosing = true;

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            StopCapturing();

            base.OnClosed(e);
        }
    }
}
