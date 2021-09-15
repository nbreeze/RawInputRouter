using Microsoft.UI.Xaml;
using PInvoke;
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
        public override void ProcessWindowMessage(IntPtr wParam, IntPtr lParam)
        {
            base.ProcessWindowMessage(wParam, lParam);

            WinMsgIntercept.GetCBT(out var cbt);

            switch (cbt.Code)
            {
                case 3: // HCBT_CREATEWND
                    {
                        bool waitForUpdate = false;

                        foreach (var app in Applications)
                        {
                            if (User32.IsWindow(app.Handle))
                                continue;

                            // We need to wait a little bit before checking the window.
                            waitForUpdate = true;
                            break;
                        }

                        if (waitForUpdate)
                        {
                            Task.Delay(5000).ContinueWith(task =>
                            {
                                // Make sure we're running on UI thread.
                                Window.Current.DispatcherQueue.TryEnqueue(() =>
                                {
                                    foreach (var app in Applications)
                                    {
                                        if (User32.IsWindow(app.Handle))
                                            continue;

                                        app.FindWindow();
                                    }
                                });
                            });
                        }
                    }

                    break;
            }
        }
    }
}
