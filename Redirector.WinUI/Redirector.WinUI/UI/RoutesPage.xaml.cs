using Microsoft.UI.Xaml;
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
    public class RouteGroup : List<IRoute>
    {
        public RouteGroup(IEnumerable<IRoute> items) : base(items)
        {
        }

        public IDeviceSource Source { get; set; }
    }

    public class RouteTriggerDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return base.SelectTemplateCore(item);
        }
    }

    public class RouteActionDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return base.SelectTemplateCore(item);
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RoutesPage : Page
    {
        public ObservableCollection<IRoute> Routes => App.Current.Redirector.Routes;

        public static ObservableCollection<IApplicationReceiver> Applications => App.Current.Redirector.Applications;

        public RoutesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.Current.ViewModel.TopLevelHeader = "Routes";

            Routes.CollectionChanged += OnRoutesCollectionChanged;
            RoutesCVS.Source = GetGroupedRoutesAsync();
        }

        private void OnRoutesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RoutesCVS.Source = GetGroupedRoutesAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Routes.CollectionChanged -= OnRoutesCollectionChanged;
        }

        private ObservableCollection<RouteGroup> GetGroupedRoutesAsync()
        {
            var query = from item in Routes
                        group item by item.Source into deviceGroup
                        select new RouteGroup(deviceGroup) { Source = deviceGroup.Key };

            return new ObservableCollection<RouteGroup>(query);
        }

        private async void OnClickAddRouteButton(object sender, RoutedEventArgs e)
        {
            WinUIRoute route = new();

            NewRouteDialog dialog = new()
            { 
                Source = route,
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    Routes.Add(route);
                    break;
            }
        }

        private async void OnClickDeleteRouteMenuButton(object sender, RoutedEventArgs e)
        {
            var route = (sender as FrameworkElement).Tag as WinUIRoute;

            ContentDialog dialog = new()
            {
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                Title = "Delete route",
                Content = $"Are you sure you want to delete the route? This action is irreversible!",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    Routes.Remove(route);
                    break;
            }
        }

        private async void OnClickEditDeviceButton(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            if (element.Tag is not WinUIDeviceSource dest)
                return;

            WinUIDeviceSource source = new();
            source.Copy(dest);

            var dialog = new DeviceSettingsDialog()
            {
                Title = "Edit device source",
                Source = source,
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    dest.Copy(source);
                    break;
            }
        }

        private void OnClickAddRouteTrigger(object sender, RoutedEventArgs e)
        {

        }

        private void OnClickAddRouteAction(object sender, RoutedEventArgs e)
        {

        }
    }
}
