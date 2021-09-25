using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.WinUI.UI
{
    internal class NavigationItem
    {
        public string Caption { get; set; } = "";
        public Symbol Glyph { get; set; }
        public Type PageType { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopLevelPage : Page
    {
        private NavigationItem[] NavigationItems { get; } =
        {
            new() { Caption = "Devices", Glyph = Symbol.Keyboard, PageType = typeof(DevicesPage) },
            new() { Caption = "Applications", Glyph = Symbol.Caption, PageType = typeof(ApplicationsPage) },
            new() { Caption = "Routes", Glyph = Symbol.Remote, PageType = typeof(RoutesPage) },
        };

        public TopLevelPage()
        {
            this.InitializeComponent();

            Navigation.SelectedItem = NavigationItems.First();

            ContentFrame.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            // Sync the selected navigation item.
            NavigationItem item = NavigationItems.Where(i => i.PageType == e.SourcePageType)
                .FirstOrDefault();

            if (item != null)
            {
                Navigation.SelectedItem = item;
            }
            else if (e.SourcePageType == typeof(SettingsPage))
            {
                Navigation.SelectedItem = Navigation.SettingsItem;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.Current.TopLevelFrame = ContentFrame;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (App.Current.TopLevelFrame == ContentFrame)
                App.Current.TopLevelFrame = null;
        }

        private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == null)
                return;
            if (args.SelectedItem is NavigationItem item)
            {
                if (ContentFrame.SourcePageType == item.PageType)
                    return;

                ContentFrame.Navigate(item.PageType);
            }
            else if (args.SelectedItem == Navigation.SettingsItem)
            {
                if (ContentFrame.SourcePageType == typeof(SettingsPage))
                    return;

                ContentFrame.Navigate(typeof(SettingsPage));
            }
        }

        private void OnNavigationBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }
    }
}
