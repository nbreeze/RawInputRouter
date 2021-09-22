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
using Redirector.WinUI.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.WinUI.UI
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

    public sealed partial class NewRouteTriggerDialog : ContentDialog
    {
        public static NewRouteTriggerDialog Current { get; private set; }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), 
            typeof(string), 
            typeof(NewRouteTriggerDialog), 
            new("")
        );

        public string SelectedItem { get => GetValue(SelectedItemProperty) as string; set => SetValue(SelectedItemProperty, value); }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(IWinUIRouteTrigger),
            typeof(NewRouteTriggerDialog),
            new(null)
        );

        public IWinUIRouteTrigger Source { get => GetValue(SourceProperty) as IWinUIRouteTrigger; set => SetValue(SourceProperty, value); }

        public IWinUIRouteTrigger Destination { get; set; }

        public static readonly DependencyProperty IsCapturingKeyboardInputProperty = DependencyProperty.Register(
            nameof(IsCapturingKeyboardInput),
            typeof(bool),
            typeof(NewRouteTriggerDialog),
            new(false)
        );

        public bool IsCapturingKeyboardInput { get => (bool)GetValue(IsCapturingKeyboardInputProperty); set => SetValue(IsCapturingKeyboardInputProperty, value); }

        public static Dictionary<string, Type> RouteTriggerTypesDictionary = new Dictionary<string, Type>()
        {
            { "Keyboard Input", typeof(WinUIKeyboardInputRouteTrigger) }
        };

        public NewRouteTriggerDialog()
        {
            Current = this;

            this.InitializeComponent();

            Closed += OnClosed;
            App.Current.Redirector.Input += OnRedirectorInput;

            if (Destination != null)
            {
                SelectedItem = RouteTriggerTypesDictionary.First(pair => pair.Value == Destination.GetType())
                    .Key;
            }
            else
            {
                SelectedItem = RouteTriggerTypesDictionary.Keys.First();
            }
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
            string routeTriggerTypeName = e.AddedItems.FirstOrDefault() as string;
            if (string.IsNullOrEmpty(routeTriggerTypeName))
            {
                Source = null;
                return;
            }

            IWinUIRouteTrigger newSource = Activator.CreateInstance(RouteTriggerTypesDictionary[routeTriggerTypeName]) as IWinUIRouteTrigger;
            if (newSource != null)
            {
                if (Destination != null && newSource.GetType() == Destination.GetType())
                {
                    newSource.Copy(Destination);
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
