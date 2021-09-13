using System;
using System.Windows;

namespace RawInputRouter
{
    /// <summary>
    /// Interaction logic for ProcessWindowDialog.xaml
    /// </summary>
    public partial class ProcessWindowDialog : DialogBase
    {
        public static readonly DependencyProperty TemporaryProcessWindowProperty = DependencyProperty.Register(
            "TemporaryProcessWindow",
            typeof(RIRApplicationReceiver),
            typeof(ProcessWindowDialog),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty ProcessWindowProperty = DependencyProperty.Register(
            "ProcessWindow",
            typeof(RIRApplicationReceiver),
            typeof(ProcessWindowDialog),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public delegate bool VerifyResultDelegate(RIRApplicationReceiver tempWindow, RIRApplicationReceiver window, ref string errorText);
        public event VerifyResultDelegate VerifyResult;

        public delegate void AcceptResultDelegate(RIRApplicationReceiver tempWindow, RIRApplicationReceiver window);
        public event AcceptResultDelegate AcceptResult;

        public RIRApplicationReceiver ProcessWindow { get => (RIRApplicationReceiver)GetValue(ProcessWindowProperty); set => SetValue(ProcessWindowProperty, value); }

        public RIRApplicationReceiver TemporaryProcessWindow { get => (RIRApplicationReceiver)GetValue(TemporaryProcessWindowProperty); set => SetValue(TemporaryProcessWindowProperty, value); }

        public ProcessWindowDialog() : base()
        {
            InitializeComponent();

            SearchMethodComboBox.ItemsSource = Enum.GetValues(typeof(WindowTitleSearchMethod));
            TemporaryProcessWindow = new RIRApplicationReceiver()
            {
                ExecutableName = "",
                Name = "",
                WindowTitleSearchMethod = WindowTitleSearchMethod.Exact,
                WindowTitleSearch = ""
            };

            TemporaryProcessWindow.PropertyChanged += OnTemporaryProcessWindowPropertyChanged;
        }

        private void OnTemporaryProcessWindowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ( e.PropertyName == "ExecutableName" || e.PropertyName == "WindowTitleSearch" || e.PropertyName == "WindowTitleSearchMethod" )
            {
                TemporaryProcessWindow.FindWindow();
            }
        }

        protected override void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TemporaryProcessWindow.Name.Trim()))
            {
                ErrorText = "Name cannot be blank.";
                return;
            }

            if (string.IsNullOrEmpty(TemporaryProcessWindow.ExecutableName.Trim()))
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
