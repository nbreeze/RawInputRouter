using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.App.UI
{
    public sealed partial class ApplicationSettingsDialog : ContentDialog
    {
        public WindowTextSearch[] EnumType = Enum.GetValues<WindowTextSearch>();

        public WinUIApplicationReceiver Source { get; set; }

        public ApplicationSettingsDialog()
        {
            this.InitializeComponent();
        }
    }

    public class WindowTextSearchNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            WindowTextSearch search = (WindowTextSearch)value;
            switch (search)
            {
                case WindowTextSearch.Any:
                    return "Any";
                case WindowTextSearch.Exact:
                    return "Exact";
                case WindowTextSearch.Contains:
                    return "Contains";
            }

            return search.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
