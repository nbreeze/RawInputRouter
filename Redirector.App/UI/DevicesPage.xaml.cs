using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.App.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DevicesPage : Page
    {
        private ObservableCollection<IDeviceSource> Sources => App.Current.Redirector.Devices;

        public DevicesPage()
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
            WinUIDeviceSource source = (sender as FrameworkElement).DataContext as WinUIDeviceSource;

            if (source.Handle == IntPtr.Zero)
            {
                source.Handle = source.FindHandle();
            }
        }

        private async void OnClickEditDeviceMenuButton(object sender, RoutedEventArgs e)
        {
            WinUIDeviceSource source = (sender as FrameworkElement).DataContext as WinUIDeviceSource;

            await ShowDeviceSettingsDialog(source);
        }

        private async void OnClickDeleteDeviceMenuButton(object sender, RoutedEventArgs e)
        {
            WinUIDeviceSource source = (sender as FrameworkElement).DataContext as WinUIDeviceSource;

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

        private void OnDeviceContextFlyoutOpening(object _sender, object e)
        {
            FlyoutBase sender = (FlyoutBase)_sender;
            var dataContext = sender.Target?.DataContext ?? (sender.Target as ContentControl)?.Content;

            IEnumerable<MenuFlyoutItemBase> items = ((MenuFlyout)sender).Items;

            if (items != null)
            {
                foreach (MenuFlyoutItemBase item in items)
                {
                    item.DataContext = dataContext;
                }
            }
        }
    }
}
