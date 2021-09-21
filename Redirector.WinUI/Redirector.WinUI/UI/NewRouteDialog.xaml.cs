﻿using Microsoft.UI.Xaml;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.WinUI.UI
{
    public sealed partial class NewRouteDialog : ContentDialog
    {
        public static ObservableCollection<IDeviceSource> Devices => App.Current.Redirector.Devices;

        public static ObservableCollection<IApplicationReceiver> Applications => App.Current.Redirector.Applications;

        public WinUIRoute Source { get; set; }

        public NewRouteDialog()
        {
            this.InitializeComponent();
        }
    }
}
