using Microsoft.UI.Xaml;
using PInvoke;
using Redirector.Core;
using Redirector.Core.Windows;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.WinUI
{
    public class WinUIRedirector : Win32Redirector
    {
        public override void OnWindowTextChanged(IntPtr hWnd)
        {
            foreach (IApplicationReceiver _application in Applications)
            {
                if (_application is WinUIApplicationReceiver application)
                {
                    application.OnWindowTextChanged(hWnd);
                }
            }

            base.OnWindowTextChanged(hWnd);
        }
    }
}
