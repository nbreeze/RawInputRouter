using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RawInputRouter
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(IntPtr), typeof(string))]
    public class IntPtrToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IntPtr))
                throw new InvalidOperationException("The value must be an IntPtr");

            return "0x" + ((IntPtr)value).ToString("x");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class IgnoreMousewheelUnlessFocusedBehavior : DependencyObject
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
          "Enabled",
          typeof(bool),
          typeof(IgnoreMousewheelUnlessFocusedBehavior),
          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnEnabledChanged))
        );

        public static readonly DependencyProperty IsCustomFocusedProperty = DependencyProperty.RegisterAttached(
          "IsCustomFocused",
          typeof(bool),
          typeof(IgnoreMousewheelUnlessFocusedBehavior),
          new FrameworkPropertyMetadata(false)
        );

        // These get/set accessors have to be provided to be used in XAML.
        public static bool GetEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(IsCustomFocusedProperty, false);

            if ( (bool)e.NewValue )
            {
                if (d is UIElement)
                {
                    ((UIElement)d).MouseEnter += MouseEnter;
                    ((UIElement)d).PreviewMouseWheel += PreviewMouseWheel;
                    ((UIElement)d).PreviewMouseDown += PreviewMouseDown;
                }
            }
            else
            {
                if (d is UIElement)
                {
                    ((UIElement)d).MouseEnter -= MouseEnter;
                    ((UIElement)d).PreviewMouseWheel -= PreviewMouseWheel;
                    ((UIElement)d).PreviewMouseDown -= PreviewMouseDown;
                }
            }
        }

        private static void MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var element = (UIElement)sender;

            element.SetValue(IsCustomFocusedProperty, false);
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private static void PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var element = (UIElement)sender;
            if (!GetEnabled(element)) return;

            if ((bool)element.GetValue(IsCustomFocusedProperty))
            {
                bool doDefaultBehavior = true;
                if (element is ListView)
                {
                    ScrollViewer sv = FindVisualChild<ScrollViewer>(element);
                    if (sv != null && sv.ComputedVerticalScrollBarVisibility == Visibility.Collapsed)
                    {
                        doDefaultBehavior = false;
                    }
                }

                if (doDefaultBehavior) return;
            }

            // Pass on the event.
            e.Handled = true;
            var e2 = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            e2.RoutedEvent = UIElement.MouseWheelEvent;
            element.RaiseEvent(e2);
        }

        private static void PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            if (!GetEnabled(element)) return;

            element.SetValue(IsCustomFocusedProperty, true);
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ListView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
