using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PInvoke;
using Redirector.App.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.App.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.Current.ViewModel.TopLevelHeader = "Settings";
        }

        private async void OnClickImportSettings(object sender, RoutedEventArgs e)
        {
            FileOpenPicker dialog = new();
            dialog.FileTypeFilter.Add(".json");

            // Pain.
            // https://github.com/microsoft/microsoft-ui-xaml/issues/2716
            if (Window.Current == null)
            {
                IInitializeWithWindow initializeWithWindowWrapper = dialog.As<IInitializeWithWindow>();
                IntPtr hwnd = User32.GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);
            }

            StorageFile file = await dialog.PickSingleFileAsync();
            if (file != null)
            {
                using Stream stream = await file.OpenStreamForReadAsync();

                JsonSerializerOptions options = new()
                {
                    Converters =
                    {
                        new WinUIRedirectorSerializedDataJsonConverter()
                    }
                };

                WinUIRedirectorSerializedData data = await JsonSerializer.DeserializeAsync<WinUIRedirectorSerializedData>(stream, options);

                var redirector = App.Current.Redirector;
                redirector.Devices.Clear();
                redirector.Applications.Clear();
                
                foreach (WinUIDeviceSource source in data.Devices)
                {
                    redirector.Devices.Add(source);
                    source.Handle = source.FindHandle();
                }

                foreach (WinUIApplicationReceiver app in data.Applications)
                {
                    redirector.Applications.Add(app);
                    app.FindWindow();
                }
            }
        }

        private async void OnClickExportSettings(object sender, RoutedEventArgs e)
        {
            FileSavePicker dialog = new();
            dialog.FileTypeChoices.Add("JSON Text File", new List<string>() { ".json" } );

            if (Window.Current == null)
            {
                IInitializeWithWindow initializeWithWindowWrapper = dialog.As<IInitializeWithWindow>();
                IntPtr hwnd = User32.GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);
            }

            StorageFile file = await dialog.PickSaveFileAsync();
            if (file != null)
            {
                var redirector = App.Current.Redirector;
                WinUIRedirectorSerializedData data = new(redirector.Devices, redirector.Applications, redirector.Routes);
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true,
                    Converters =
                    {
                        new WinUIRedirectorSerializedDataJsonConverter()
                    }
                };

                string json = JsonSerializer.Serialize(data, options);

                await File.WriteAllTextAsync(file.Path, json);
            }
        }
    }
}
