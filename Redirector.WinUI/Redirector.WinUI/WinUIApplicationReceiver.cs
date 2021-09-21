using Microsoft.UI.Xaml;
using PInvoke;
using Redirector.Core;
using Redirector.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.WinUI
{
    public enum WindowTextSearch
    {
        Any,
        Exact,
        Contains
    }

    public class WinUIApplicationReceiver : Win32ApplicationReceiver
    {
        private string _WindowTextSearchQuery = "";
        public string WindowTextSearchQuery { get => _WindowTextSearchQuery; set => SetProperty(ref _WindowTextSearchQuery, value); }

        private WindowTextSearch _WindowTextSearch = WindowTextSearch.Any;
        public WindowTextSearch WindowTextSearch { 
            get => _WindowTextSearch; 
            set => SetProperty(ref _WindowTextSearch, value); 
        }

        private bool _LockOnFoundWindow;
        public bool LockOnFoundWindow { get => _LockOnFoundWindow; set => SetProperty(ref _LockOnFoundWindow, value); }

        private bool _WindowTextSearchCaseSensitive;
        public bool WindowTextSearchCaseSensitive { get => _WindowTextSearchCaseSensitive; set => SetProperty(ref _WindowTextSearchCaseSensitive, value); }

        public string WindowText 
        { 
            get
            {
                if (Handle == IntPtr.Zero || !User32.IsWindow(Handle))
                    return "<Window Not Found>";
                char[] _title = new char[260];
                User32.GetWindowText(Handle, _title, _title.Length);
                return new string(_title).Replace("\0", null);
            }
        }

        public WinUIApplicationReceiver() : base()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public WinUIApplicationReceiver(WinUIApplicationReceiver source) : this()
        {
            Copy(source);
        }

        public void Copy(WinUIApplicationReceiver source)
        {
            Name = source.Name;
            ExecutableName = source.ExecutableName;
            Handle = source.Handle;
            ProcessId = source.ProcessId;
            WindowTextSearch = source.WindowTextSearch;
            WindowTextSearchQuery = source.WindowTextSearchQuery;
            WindowTextSearchCaseSensitive = source.WindowTextSearchCaseSensitive;
            LockOnFoundWindow = source.LockOnFoundWindow;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WindowTextSearchQuery):
                case nameof(WindowTextSearch):
                case nameof(WindowTextSearchCaseSensitive):
                    FindWindow();
                    break;
                case nameof(Handle):
                    OnPropertyChanged(nameof(WindowText));
                    break;
            }
        }

        public override bool IsMatchingWindow(IntPtr handle)
        {
            switch (_WindowTextSearch)
            {
                case WindowTextSearch.Any:
                    return true;
                case WindowTextSearch.Exact:
                case WindowTextSearch.Contains:
                    if (!string.IsNullOrEmpty(_WindowTextSearchQuery))
                    {
                        char[] _windowTitle = new char[260];
                        User32.GetWindowText(handle, _windowTitle, _windowTitle.Length);
                        string windowTitle = new string(_windowTitle).Replace("\0", null);

                        if (_WindowTextSearch == WindowTextSearch.Exact)
                        {
                            return windowTitle.Equals(_WindowTextSearchQuery, _WindowTextSearchCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
                        }
                        else if (_WindowTextSearch == WindowTextSearch.Contains)
                        {
                            return windowTitle.Contains(_WindowTextSearchQuery, _WindowTextSearchCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                    return false;
            }

            return false;
        }

        public virtual void OnWindowTextChanged(IntPtr handle)
        {
            if (handle == Handle)
            {
                if (!LockOnFoundWindow)
                {
                    if (!IsMatchingWindow(handle))
                    {
                        FindWindow();
                    }
                }
                else
                {
                    OnPropertyChanged(nameof(WindowText));
                }
            }
        }

        public string DisplayHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero || !User32.IsWindow(handle))
                return "";
            return string.Format("0x{0:X}", handle);
        }
    }
}
