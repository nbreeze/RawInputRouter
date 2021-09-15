﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Redirector.WinUI.UI.Home;
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
    public class NavigationItem
    {
        public string Title { get; set; } = "";
        public IconElement Icon { get; set; } = null;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopLevelPage : Page
    {
        public ICollection<NavigationItem> NavigationItems { get; } = new Collection<NavigationItem>()
        {
            new() { Title = "Devices", Icon = new SymbolIcon(Symbol.Keyboard) },
            new() { Title = "Applications", Icon = new SymbolIcon(Symbol.Caption) },
            new() { Title = "Routing", Icon = new SymbolIcon(Symbol.Remote) },
        };

        public TopLevelPage()
        {
            this.InitializeComponent();

            Navigation.SelectedItem = NavigationItems.First();
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
            if (args.SelectedItem is not NavigationItem item)
                return;

            switch (item.Title)
            {
                case "Home":
                    break;
                case "Devices":
                    ContentFrame.Navigate(typeof(HomePage));
                    break;
                case "Applications":
                    break;
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