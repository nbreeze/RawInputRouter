using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RawInputRouter
{
    public class DialogBase : Window
    {
        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(
            "ErrorText",
            typeof(string),
            typeof(DialogBase),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public string ErrorText { get => (string)GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }

        public bool IsClosing { get; private set; }

        public DialogBase() : base()
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            IsClosing = true;

            base.OnClosing(e);
        }

        protected virtual void OKButton_Click(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
