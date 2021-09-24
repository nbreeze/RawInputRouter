using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.WinUI.UI
{
    public class ObjectContentDialog : ContentDialog
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(object),
            typeof(NewRouteActionDialog),
            new(null)
        );

        public object Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }

        public object Destination { get; set; }
    }
}
