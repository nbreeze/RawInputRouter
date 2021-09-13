using PInvoke;
using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace RawInputRouter
{
    public enum WindowTitleSearchMethod
    {
        Exact,
        Match,
        Regex
    }

    public class ProcessWindowTitleSearchEnumConverter : IValueConverter
    {
        internal static Dictionary<WindowTitleSearchMethod, string> ProcessWindowTitleSearchNames = new Dictionary<WindowTitleSearchMethod, string>()
            {
                { WindowTitleSearchMethod.Exact, "Exact" },
                { WindowTitleSearchMethod.Match, "Match" },
                { WindowTitleSearchMethod.Regex, "Regex" }
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = "";
            ProcessWindowTitleSearchNames.TryGetValue((WindowTitleSearchMethod)value, out name);
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

        private WindowTitleSearchMethod _WindowTitleSearchMethod;
        public WindowTitleSearchMethod WindowTitleSearchMethod { get => _WindowTitleSearchMethod; set => SetProperty(ref _WindowTitleSearchMethod, value); }

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
            if (WindowTitleSearchMethod != WindowTitleSearchMethod.Regex || string.IsNullOrEmpty(WindowTitleSearch))
            {
                _Regex = null;
                return;
            }

            RegexOptions regexFlags = RegexOptions.None;
            _Regex = new Regex(WindowTitleSearch, regexFlags);
        }

        public override bool IsMatchingWindow(IntPtr handle)
        {
            char[] windowTitle = new char[260];
            User32.GetWindowText(handle, windowTitle, windowTitle.Length);

            string title = new string(windowTitle).Replace("\0", "");

            switch (WindowTitleSearchMethod)
            {
                case WindowTitleSearchMethod.Exact:
                    return title.Equals(WindowTitleSearch);
                case WindowTitleSearchMethod.Match:
                    return title.Contains(WindowTitleSearch);
                case WindowTitleSearchMethod.Regex:
                    return _Regex != null && _Regex.IsMatch(title.ToString());
                default:
                    break;
            }

            return false;
        }
    }
}
