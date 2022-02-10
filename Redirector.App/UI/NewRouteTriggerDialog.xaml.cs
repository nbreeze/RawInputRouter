using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Redirector.Core;
using Redirector.Core.Windows;
using Redirector.Core.Windows.Triggers;
using Redirector.App;
using Redirector.App.Triggers;
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
    internal class RouteTriggerDialogContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate KeyboardInputTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is IWinUIRouteTrigger trigger)
            {
                Type triggerType = trigger.GetType();
                if (triggerType == typeof(WinUIKeyboardInputRouteTrigger))
                {
                    return KeyboardInputTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    internal class RouteTriggerTypeOption
    {
        public string Name { get; set; }

        public Type Type { get; set; }
    }

    public sealed partial class NewRouteTriggerDialog : ObjectContentDialog
    {
        public static NewRouteTriggerDialog Current { get; private set; }

        public static readonly DependencyProperty IsCapturingKeyboardInputProperty = DependencyProperty.Register(
            nameof(IsCapturingKeyboardInput),
            typeof(bool),
            typeof(NewRouteTriggerDialog),
            new(false)
        );

        public bool IsCapturingKeyboardInput { get => (bool)GetValue(IsCapturingKeyboardInputProperty); set => SetValue(IsCapturingKeyboardInputProperty, value); }

        private IList<RouteTriggerTypeOption> Options { get; } = new List<RouteTriggerTypeOption>()
        {
            new() { Name = "Keyboard input", Type = typeof(WinUIKeyboardInputRouteTrigger) }
        };

        public NewRouteTriggerDialog()
        {
            Current = this;

            this.InitializeComponent();

            Closed += OnClosed;
            App.Current.Redirector.Input += OnRedirectorInput;

            RouteTriggerTypeOption option = null;

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

        private void OnRedirectorInput(object sender, RedirectorInputEventArgs e)
        {
            if (Source == null)
                return;

            if (IsCapturingKeyboardInput && e.Input is Win32KeyboardDeviceInput kbInput && kbInput.IsKeyDown)
            {
                IsCapturingKeyboardInput = false;

                if (Source is WinUIKeyboardInputRouteTrigger trigger)
                {
                    trigger.VKey = kbInput.VKey;
                }
            }
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            IsCapturingKeyboardInput = false;

            App.Current.Redirector.Input -= OnRedirectorInput;

            if (Current == this)
                Current = null;
        }

        private void OnSelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            RouteTriggerTypeOption option = e.AddedItems.FirstOrDefault() as RouteTriggerTypeOption;
            if (option == null || option.Type == null)
            {
                Source = null;
                return;
            }

            IWinUIRouteTrigger newSource = Activator.CreateInstance(option.Type) as IWinUIRouteTrigger;
            if (newSource != null)
            {
                if (Destination != null && newSource.GetType() == Destination.GetType())
                {
                    newSource.Copy(Destination as IWinUIRouteTrigger);
                }
            }

            Source = newSource;
        }

        private void OnClickKeyboardTriggerCapture(object sender, RoutedEventArgs e)
        {
            IsCapturingKeyboardInput = !IsCapturingKeyboardInput;
        }

        public static string GetKeyboardTriggerCaptureButtonText(bool capturing)
        {
            return capturing ? "Cancel" : "Capture Key";
        }
    }
}
