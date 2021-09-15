using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows
{
    public interface IWin32Redirector : IRedirector
    {
        public void ProcessRawInputDeviceChangedMessage(IntPtr wParam, IntPtr lParam);

        public bool ProcessRawInputMessage(IntPtr wParam, IntPtr lParam);

        public void ProcessWindowMessage(IntPtr wParam, IntPtr lParam);

        public bool ProcessKeyboardInterceptMessage(IntPtr wParam, IntPtr lParam);

        public void OnWindowTextChanged(IntPtr hWnd);
    }
}
