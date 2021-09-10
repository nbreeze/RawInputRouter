using RawInputRouter.Imports;
using RawInputRouter.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RawInputRouter
{
    public enum ProcessWindowTitleSearch
    {
        Exact,
        Match,
        Regex
    }

    public class ProcessWindowTitleSearchEnumConverter : IValueConverter
    {
        internal static Dictionary<ProcessWindowTitleSearch, string> ProcessWindowTitleSearchNames = new Dictionary<ProcessWindowTitleSearch, string>()
            {
                { ProcessWindowTitleSearch.Exact, "Exact" },
                { ProcessWindowTitleSearch.Match, "Match" },
                { ProcessWindowTitleSearch.Regex, "Regex" }
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = "";
            ProcessWindowTitleSearchNames.TryGetValue((ProcessWindowTitleSearch)value, out name);
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RIRApplicationReceiver : ApplicationReceiver
    {
        private string _WindowTitleSearch;
        public string WindowTitleSearch { get => _WindowTitleSearch; set => SetProperty(ref _WindowTitleSearch, value); }

        private ProcessWindowTitleSearch _WindowTitleSearchMethod;
        public ProcessWindowTitleSearch WindowTitleSearchMethod { get => _WindowTitleSearchMethod; set => SetProperty(ref _WindowTitleSearchMethod, value); }

        private Regex _Regex = null;

        public RIRApplicationReceiver() : base()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowTitleSearchMethod" || e.PropertyName == "WindowTitleSearch")
            {
                UpdateRegex();
            }
        }

        private void UpdateRegex()
        {
            if (WindowTitleSearchMethod != ProcessWindowTitleSearch.Regex || string.IsNullOrEmpty(WindowTitleSearch))
            {
                _Regex = null;
                return;
            }

            RegexOptions regexFlags = RegexOptions.None;
            _Regex = new Regex(WindowTitleSearch, regexFlags);
        }

        public override bool IsMatchingWindow(IntPtr handle)
        {
            StringBuilder windowTitle = new StringBuilder(260);
            User32.GetWindowText(handle, windowTitle, 260);

            switch (WindowTitleSearchMethod)
            {
                case ProcessWindowTitleSearch.Exact:
                    return windowTitle.ToString().Equals(WindowTitleSearch);
                case ProcessWindowTitleSearch.Match:
                    return windowTitle.ToString().Contains(WindowTitleSearch);
                case ProcessWindowTitleSearch.Regex:
                    return _Regex != null && _Regex.IsMatch(windowTitle.ToString());
                default:
                    break;
            }

            return false;
        }
    }
}
