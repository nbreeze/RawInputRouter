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
        public ReadOnlyObservableCollection<IDeviceSource> Sources { get; } = new ReadOnlyObservableCollection<IDeviceSource>(App.Current.Redirector.Devices);

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.Current.ViewModel.TopLevelHeader = "Devices";
        }

        private Visibility ShouldShowNoDevicesPage(int sourcesCount)
        {
            return sourcesCount > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task ShowDeviceSettingsDialog(WinUIDeviceSource destinationSource = null)
        {
            var dialog = new DeviceSettingsDialog()
            {
                Source = new WinUIDeviceSource()
                {
                    Name = $"Device {App.Current.Redirector.Devices.Count + 1}"
                },
                DestinationSource = destinationSource,
                IsEditing = destinationSource != null,
                XamlRoot = Content.XamlRoot
            };

            if (dialog.IsEditing)
            {
                dialog.Title = "Edit Device Source";

                dialog.Source.Name = destinationSource.Name;
                dialog.Source.Path = destinationSource.Path;
                dialog.Source.Handle = destinationSource.Handle;
                dialog.Source.BlockOriginalInput = destinationSource.BlockOriginalInput;
            }
            else
            {
                dialog.Title = "Add Device Source";
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (dialog.IsEditing)
                {
                    destinationSource.Name = dialog.Source.Name;
                    destinationSource.Path = dialog.Source.Path;
                    destinationSource.Handle = dialog.Source.Handle;
                    destinationSource.BlockOriginalInput = dialog.Source.BlockOriginalInput;
                }
                else
                {
                    App.Current.Redirector.Devices.Add(dialog.Source);
                }
            }
        }

        private async void OnClickNewDeviceHyperlink(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await ShowDeviceSettingsDialog();
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
                Title = "Delete Device Source",
                Content = $"Are you sure you want to delete '{source.Name}'? This action is irreversible!",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    App.Current.Redirector.Devices.Remove(source);
                    break;
            }
        }
    }
}
