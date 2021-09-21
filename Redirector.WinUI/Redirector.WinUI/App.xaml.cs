using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using PInvoke;
using Redirector.Core.Windows;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Redirector.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => Application.Current as App;

        public MainViewModel ViewModel { get; } = new MainViewModel();

        public IWin32Redirector Redirector { get; } = new WinUIRedirector();

        public Frame TopLevelFrame { get; set; }

        private static IntPtr _OldWndProc;
        private static IntPtr _WndProc;
        private static unsafe User32.WndProc _WndProcDelegate = new(WndProc);
        private static User32.SafeEventHookHandle _WinEventProc;
        private static User32.WinEventProc _WinEventProcDelegate = new(WndEventProc);

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow main = new();

            m_window = main;
            m_window.Activate();

            using Process process = Process.GetCurrentProcess();
            IntPtr windowHandle = main.As<IWindowNative>().WindowHandle;

            if (!RawInput.RegisterRawInputDevices(new[]
                {
                    // Keyboard
                    new RawInput.RAWINPUTDEVICE()
                    {
                        UsagePage = RawInput.HIDUsagePage.Generic,
                        Usage = RawInput.HIDUsage.Keyboard,
                        Flags = RawInput.RawInputDeviceFlags.InputSink |  RawInput.RawInputDeviceFlags.DevNotify,
                        WindowHandle = windowHandle
                    }
                }, 1, Marshal.SizeOf<RawInput.RAWINPUTDEVICE>()))
            {
                User32.MessageBox(windowHandle, "FATAL ERROR! RegisterRawInputDevices failed!", null, 0);
                Exit();
                return;
            }

            if (!WinMsgIntercept.Install(windowHandle))
            {
                User32.MessageBox(windowHandle, "FATAL ERROR! WinMsgIntercept install failed!", null, 0);
                Exit();
                return;
            }

            // Subclass the main window's WndProc to intercept messages.
            _WndProc = Marshal.GetFunctionPointerForDelegate(_WndProcDelegate);
            _OldWndProc = User32.SetWindowLongPtr(windowHandle, User32.WindowLongIndexFlags.GWLP_WNDPROC, _WndProc);

            // Listen to window name change events.
            _WinEventProc = User32.SetWinEventHook(User32.WindowsEventHookType.EVENT_OBJECT_NAMECHANGE, User32.WindowsEventHookType.EVENT_OBJECT_NAMECHANGE,
                IntPtr.Zero, Marshal.GetFunctionPointerForDelegate(_WinEventProcDelegate), 0, 0, User32.WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT);

            if (_WinEventProc.IsInvalid)
            {
                User32.MessageBox(windowHandle, "FATAL ERROR! Failed to hook onto EVENT_OBJECT_NAMECHANGE!", null, 0);
                Exit();
                return;
            }
        }

        private Window m_window;

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, User32.WindowMessage msg, IntPtr wParam, IntPtr lParam);

        private static unsafe IntPtr WndProc(IntPtr hWnd, User32.WindowMessage msg, void* _wParam, void* _lParam)
        {
            IntPtr wParam = (IntPtr)_wParam;
            IntPtr lParam = (IntPtr)_lParam;
            MainViewModel viewModel = Current.ViewModel;

            switch (msg)
            {
                case User32.WindowMessage.WM_INPUT:
                    {
                        RawInput.RAWINPUT input = new();
                        if (RawInput.GetRawInputData(lParam, ref input))
                        {
                            if (viewModel.IsCapturingDevice)
                            {
                                viewModel.InvokeCaptureDeviceEvent(Current, new(ref input));
                            }
                            else
                            {
                                Current.Redirector.ProcessRawInputMessage(wParam, lParam);
                            }
                        }
                    }

                    break;
                case (User32.WindowMessage)WinMsgIntercept.WM_HOOK_KEYBOARD_INTERCEPT:
                    {
                        if (viewModel.IsCapturingDevice)
                        {
                            return (IntPtr)1;
                        }
                        else
                        {
                            if (Current.Redirector.ProcessKeyboardInterceptMessage(wParam, lParam))
                            {
                                return (IntPtr)1;
                            }
                        }
                    }

                    break;

                case (User32.WindowMessage)WinMsgIntercept.WM_HOOK_CBT: // Window activity hook
                    Current.Redirector.ProcessWindowMessage(wParam, lParam);
                    break;
            }

            if (_OldWndProc != IntPtr.Zero)
                return CallWindowProc(_OldWndProc, hWnd, msg, wParam, lParam);

            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private const int OBJID_WINDOW = 0;
        private const int CHILDID_SELF = 0;

        private static void WndEventProc(IntPtr hHook, User32.WindowsEventHookType eventType, IntPtr hWnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime)
        {
            switch (eventType)
            {
                case User32.WindowsEventHookType.EVENT_OBJECT_NAMECHANGE:
                    {
                        if (hWnd != IntPtr.Zero && idObject == OBJID_WINDOW && idChild == CHILDID_SELF)
                        {
                            Current.Redirector.OnWindowTextChanged(hWnd);
                        }
                        break;
                    }
            }
        }
    }
}
