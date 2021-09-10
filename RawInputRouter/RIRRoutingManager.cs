using RawInputRouter.Imports;
using System;
using System.Windows;
using System.Windows.Controls;
using RawInputRouter.Routing;
using System.Threading.Tasks;

namespace RawInputRouter
{
    public class RIRRoutingManager : RoutingManager
    {
        public RIRRoutingManager() : base()
        {
        }

        public override void ProcessWindowMessage(IntPtr wParam, IntPtr lParam)
        {
            base.ProcessWindowMessage(wParam, lParam);

            WinMsgIntercept.CBT cbt = new();
            WinMsgIntercept.GetCBT(ref cbt);

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
                                MainWindow.Instance.Dispatcher.Invoke(() =>
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

        public override void OnInput(IDeviceSource source, DeviceInput input)
        {
            base.OnInput(source, input);
        }

        protected override DeviceInput MatchBufferDeviceInput(DeviceInput input)
        {
            return base.MatchBufferDeviceInput(input);
        }

        public override bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input)
        {
            return base.ShouldBlockOriginalInput(source, input);
        }
    }

    public class InputRouteDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MouseTemplate { get; set; } = null;
        public DataTemplate KeyboardTemplate { get; set; } = null;
        public DataTemplate HidTemplate { get; set; } = null;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
    }
}
