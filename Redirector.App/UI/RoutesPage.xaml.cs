using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Redirector.Core;
using Redirector.App.Actions;
using Redirector.App.Triggers;
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
    public class RouteGroup : List<IRoute>
    {
        public RouteGroup(IEnumerable<IRoute> items) : base(items)
        {
        }

        public IDeviceSource Source { get; set; }
    }

    public class RouteTriggerDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate KeyboardInputTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is WinUIKeyboardInputRouteTrigger)
            {
                return KeyboardInputTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public class RouteActionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AcceleratorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is WinUIAcceleratorOutputAction)
            {
                return AcceleratorTemplate;
            }

            return base.SelectTemplateCore(item, container);
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

        private async void OnClickAddRouteTrigger(object sender, RoutedEventArgs e)
        {
            WinUIRoute route = ((FrameworkElement)sender).Tag as WinUIRoute;
            if (route == null)
                return;

            NewRouteTriggerDialog dialog = new()
            {
                Title = "Add route trigger",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    route.Triggers.Add((IWinUIRouteTrigger)dialog.Source);
                    break;
            }
        }

        private async void OnClickAddRouteAction(object sender, RoutedEventArgs e)
        {
            WinUIRoute route = ((FrameworkElement)sender).Tag as WinUIRoute;
            if (route == null)
                return;

            NewRouteActionDialog dialog = new()
            {
                Title = "Add route action",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    route.Actions.Add((IWinUIRouteOutputAction)dialog.Source);
                    break;
            }
        }

        private void OnRouteInnerFlyoutOpening(object _sender, object e)
        {
            CommandBarFlyout sender = (CommandBarFlyout)_sender;
            var dataContext = sender.Target?.DataContext ?? (sender.Target as ContentControl)?.Content;

            IEnumerable<ICommandBarElement> commands = sender.PrimaryCommands.Concat(sender.SecondaryCommands);

            foreach (ICommandBarElement command in commands)
            {
                if (command is not FrameworkElement obj)
                    continue;
                obj.DataContext = dataContext;
            }
        }

        private async void OnClickEditRouteComponentButton(object sender, RoutedEventArgs e)
        {
            object dataContext = ((FrameworkElement)sender).DataContext;
            if (dataContext == null)
                return;

            IRoute route = null;
            IWinUIRouteTrigger trigger = dataContext as IWinUIRouteTrigger;
            IWinUIRouteOutputAction action = dataContext as IWinUIRouteOutputAction;

            if (trigger != null)
                route = trigger.Route;
            else if (action != null)
                route = action.Route;
            else
                return;

            if (route == null)
                return;

            ObjectContentDialog dialog = null;
            if (trigger != null)
            {
                dialog = new NewRouteTriggerDialog()
                {
                    Title = "Edit route trigger",
                    Destination = dataContext,
                    XamlRoot = Content.XamlRoot
                };
            }
            else
            {
                dialog = new NewRouteActionDialog()
                {
                    Title = "Edit route action",
                    Destination = dataContext,
                    XamlRoot = Content.XamlRoot
                };
            }

            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Primary:
                    if (dataContext.GetType() == dialog.Source.GetType())
                    {
                        if (trigger != null)
                        {
                            trigger.Copy((IWinUIRouteTrigger)dialog.Source);
                        }
                        else
                        {
                            action.Copy((IWinUIRouteOutputAction)dialog.Source);
                        }
                    }
                    else
                    {
                        int index;

                        if (trigger != null)
                        {
                            index = route.Triggers.IndexOf(trigger);
                        }
                        else
                        {
                            index = route.Actions.IndexOf(action);
                        }

                        if (index != -1)
                        {
                            if ( trigger != null )
                            {
                                route.Triggers.RemoveAt(index);
                                route.Triggers.Insert(index, (IWinUIRouteTrigger)dialog.Source);
                            }
                            else
                            {
                                route.Actions.RemoveAt(index);
                                route.Actions.Insert(index, (IWinUIRouteOutputAction)dialog.Source);
                            }
                        }
                        else
                        {
                            if (trigger != null)
                            {
                                route.Triggers.Add((IWinUIRouteTrigger)dialog.Source);
                            }
                            else
                            {
                                route.Actions.Add((IWinUIRouteOutputAction)dialog.Source);
                            }
                        }
                    }

                    break;
            }
        }

        private async void OnClickDeleteRouteComponentButton(object sender, RoutedEventArgs e)
        {
            object dataContext = ((FrameworkElement)sender).DataContext;
            if (dataContext == null)
                return;

            IRoute route = null;
            IWinUIRouteTrigger trigger = dataContext as IWinUIRouteTrigger;
            IWinUIRouteOutputAction action = dataContext as IWinUIRouteOutputAction;

            if (trigger != null)
                route = trigger.Route;
            else if (action != null)
                route = action.Route;
            else
                return;

            if (route == null)
                return;

            string title;
            string content;
            if ( trigger != null )
            {
                title = "Delete route trigger";
                content = "Are you sure you want to delete this trigger? This action is irreversible!";
            }
            else
            {
                title = "Delete route action";
                content = "Are you sure you want to delete this action? This action is irreversible!";
            }

            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary:
                    if (trigger != null)
                    {
                        route.Triggers.Remove(trigger);
                    }
                    else
                    {
                        route.Actions.Remove(action);
                    }
                    break;
            }
        }
    }
}
