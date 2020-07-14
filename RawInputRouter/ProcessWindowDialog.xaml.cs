using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RawInputRouter
{
    /// <summary>
    /// Interaction logic for ProcessWindowDialog.xaml
    /// </summary>
    public partial class ProcessWindowDialog : DialogBase
    {
        public static readonly DependencyProperty TemporaryProcessWindowProperty = DependencyProperty.Register(
            "TemporaryProcessWindow",
            typeof(InputManager.ProcessWindow),
            typeof(ProcessWindowDialog),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty ProcessWindowProperty = DependencyProperty.Register(
            "ProcessWindow",
            typeof(InputManager.ProcessWindow),
            typeof(ProcessWindowDialog),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public delegate bool VerifyResultDelegate(InputManager.ProcessWindow tempWindow, InputManager.ProcessWindow window, ref string errorText);
        public event VerifyResultDelegate VerifyResult;

        public delegate void AcceptResultDelegate(InputManager.ProcessWindow tempWindow, InputManager.ProcessWindow window);
        public event AcceptResultDelegate AcceptResult;

        public InputManager.ProcessWindow ProcessWindow { get => (InputManager.ProcessWindow)GetValue(ProcessWindowProperty); set => SetValue(ProcessWindowProperty, value); }

        public InputManager.ProcessWindow TemporaryProcessWindow { get => (InputManager.ProcessWindow)GetValue(TemporaryProcessWindowProperty); set => SetValue(TemporaryProcessWindowProperty, value); }

        public ProcessWindowDialog() : base()
        {
            InitializeComponent();

            SearchMethodComboBox.ItemsSource = Enum.GetValues(typeof(ProcessWindowTitleSearch));
            TemporaryProcessWindow = new InputManager.ProcessWindow()
            {
                ExeName = "",
                Name = "",
                WindowTitleSearchMethod = ProcessWindowTitleSearch.Exact,
                WindowTitleSearch = "",
                WindowTitle = "",
                WindowHandle = IntPtr.Zero
            };

            TemporaryProcessWindow.PropertyChanged += OnTemporaryProcessWindowPropertyChanged;
        }

        private void OnTemporaryProcessWindowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ( e.PropertyName == "ExeName" || e.PropertyName == "WindowTitleSearch" || e.PropertyName == "WindowTitleSearchMethod" )
            {
                TemporaryProcessWindow.Update(true);
            }
        }

        protected override void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TemporaryProcessWindow.Name.Trim()))
            {
                ErrorText = "Name cannot be blank.";
                return;
            }

            if (string.IsNullOrEmpty(TemporaryProcessWindow.ExeName.Trim()))
            {
                ErrorText = "Executable name cannot be blank.";
                return;
            }

            ErrorText = "";

            AcceptResult?.Invoke(TemporaryProcessWindow, ProcessWindow);
            Close();
        }
    }
}
