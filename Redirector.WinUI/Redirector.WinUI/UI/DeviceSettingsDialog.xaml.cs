using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.WinUI.UI
{
    public sealed partial class DeviceSettingsDialog : ContentDialog
    {
        public WinUIDeviceSource Source { get; set; }

        public DeviceSettingsDialog()
        {
            this.InitializeComponent();

            Closed += OnClosed;
            App.Current.ViewModel.CaptureDevice += OnCaptureDevice;
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            App.Current.ViewModel.CaptureDevice -= OnCaptureDevice;
        }

        private void OnCaptureDevice(object sender, CaptureDeviceEventArgs e)
        {
            App.Current.ViewModel.IsCapturingDevice = false;

            IntPtr hDevice = e.RawInput.header.hDevice;
            Source.Handle = hDevice;
            Source.Path = RawInput.GetRawInputDeviceInterfaceName(hDevice);
        }

        private bool DisableIfCapturing(bool capturing) => !capturing;

        private void OnClickCaptureButton(object sender, RoutedEventArgs e)
        {
            if (App.Current.ViewModel.IsCapturingDevice)
                return;

            App.Current.ViewModel.IsCapturingDevice = true;
        }
    }
}
