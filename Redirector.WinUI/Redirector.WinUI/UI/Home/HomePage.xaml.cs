using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.WinUI.UI.Home
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<IDeviceSource> Sources => App.Current.Redirector.Devices;

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.Current.ViewModel.TopLevelHeader = "Devices";
        }

        private async Task ShowDeviceSettingsDialog(WinUIDeviceSource dest = null)
        {
            DeviceSettingsDialog dialog = new DeviceSettingsDialog()
            {
                Source = new WinUIDeviceSource()
                {
                    Name = $"Device {Sources.Count + 1}"
                },
                XamlRoot = Content.XamlRoot
            };

            WinUIDeviceSource source = dialog.Source;

            if (dest != null)
            {
                dialog.Title = "Edit device source";

                source.Copy(dest);
            }
            else
            {
                dialog.Title = "Add device source";
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (dest != null)
                {
                    dest.Copy(source);
                }
                else
                {
                    Sources.Add(source);
                }
            }
        }

        private async void OnClickAddDeviceButton(object sender, RoutedEventArgs e)
        {
            await ShowDeviceSettingsDialog();
        }

        private void OnClickRefreshDeviceMenuButton(object sender, RoutedEventArgs e)
        {
            WinUIDeviceSource source = (sender as FrameworkElement).Tag as WinUIDeviceSource;

            if (source.Handle == IntPtr.Zero)
            {
                source.Handle = source.FindHandle();
            }
        }

        private async void OnClickEditDeviceMenuButton(object sender, RoutedEventArgs e)
        {
            WinUIDeviceSource source = (sender as FrameworkElement).Tag as WinUIDeviceSource;

            await ShowDeviceSettingsDialog(source);
        }

        private async void OnClickDeleteDeviceMenuButton(object sender, RoutedEventArgs e)
        {
            WinUIDeviceSource source = (sender as FrameworkElement).Tag as WinUIDeviceSource;

            ContentDialog dialog = new()
            {
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                Title = "Delete device source",
                Content = $"Are you sure you want to delete '{source.Name}'? This action is irreversible!",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    Sources.Remove(source);
                    break;
            }
        }
    }
}
