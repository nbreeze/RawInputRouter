using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PInvoke;
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

namespace Redirector.WinUI.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ApplicationsPage : Page
    {
        public ObservableCollection<IApplicationReceiver> Applications => App.Current.Redirector.Applications;

        public ApplicationsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.Current.ViewModel.TopLevelHeader = "Applications";
        }

        private async Task ShowSettingsDialog(WinUIApplicationReceiver dest = null)
        {
            WinUIApplicationReceiver source;
            if (dest != null)
            {
                source = new(dest);
            }
            else
            {
                source = new()
                {
                    Name = $"Application {Applications.Count + 1}"
                };
            }

            ApplicationSettingsDialog dialog = new ApplicationSettingsDialog()
            {
                Source = source,
                XamlRoot = Content.XamlRoot
            };

            if (dest != null)
            {
                dialog.Title = "Edit application";
            }
            else
            {
                dialog.Title = "Add application";
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
                    Applications.Add(source);

                    source.FindWindow();
                }
            }
        }

        private async void OnClickAddApplicationButton(object sender, RoutedEventArgs e)
        {
            await ShowSettingsDialog();
        }

        private void OnClickRefreshApplicationMenuButton(object sender, RoutedEventArgs e)
        {
            var application = (sender as FrameworkElement).Tag as WinUIApplicationReceiver;

            if (application.Handle == IntPtr.Zero || !application.LockOnFoundWindow)
            {
                application.Handle = application.FindWindow();
            }
        }

        private async void OnClickEditApplicationMenuButton(object sender, RoutedEventArgs e)
        {
            var application = (sender as FrameworkElement).Tag as WinUIApplicationReceiver;

            await ShowSettingsDialog(application);
        }

        private async void OnClickDeleteApplicationMenuButton(object sender, RoutedEventArgs e)
        {
            var application = (sender as FrameworkElement).Tag as WinUIApplicationReceiver;

            ContentDialog dialog = new()
            {
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                Title = "Delete application",
                Content = $"Are you sure you want to delete '{application.Name}'? This action is irreversible!",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    Applications.Remove(application);
                    break;
            }
        }
    }
}
