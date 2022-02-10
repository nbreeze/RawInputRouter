using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Redirector.App.Actions;
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
    internal class RouteActionDialogContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AcceleratorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is IWinUIRouteOutputAction action)
            {
                Type actionType = action.GetType();
                if (actionType == typeof(WinUIAcceleratorOutputAction))
                {
                    return AcceleratorTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    internal class RouteActionTypeOption
    {
        public string Name { get; set; }

        public Type Type { get; set; }
    }


    public sealed partial class NewRouteActionDialog : ObjectContentDialog
    {
        private IList<RouteActionTypeOption> Options { get; } = new List<RouteActionTypeOption>()
        {
            new() { Name = "Accelerator", Type = typeof(WinUIAcceleratorOutputAction) }
        };

        public NewRouteActionDialog()
        {
            this.InitializeComponent();

            RouteActionTypeOption option = null;

            if (Destination != null)
            {
                option = Options.FirstOrDefault(option => option.Type == Destination.GetType());
            }
            
            if (option == null)
            {
                option = Options.First();
            }

            ComboBox.SelectedItem = option;
        }

        private void OnSelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            RouteActionTypeOption option = e.AddedItems.FirstOrDefault() as RouteActionTypeOption;
            if (option == null || option.Type == null)
            {
                Source = null;
                return;
            }

            IWinUIRouteOutputAction newSource = Activator.CreateInstance(option.Type) as IWinUIRouteOutputAction;
            if (newSource != null)
            {
                if (Destination is IWinUIRouteOutputAction dest && newSource.GetType() == Destination.GetType())
                {
                    newSource.Copy(dest);
                }
            }

            Source = newSource;
        }
    }
}
