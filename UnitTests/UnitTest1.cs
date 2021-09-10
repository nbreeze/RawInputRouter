using NUnit.Framework;
using RawInputRouter.Imports;
using RawInputRouter.Routing;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace UnitTests
{
    public class RoutingTests
    {
        private class TestRouter : RoutingManager
        {
        }

        private class TestWindow : ApplicationReceiver
        {
            public override bool IsMatchingWindow(IntPtr handle)
            {
                StringBuilder windowTitle = new StringBuilder(260);
                User32.GetWindowText(handle, windowTitle, windowTitle.Capacity);

                return windowTitle.ToString().Contains("Presenter View");
            }
        }

        private RoutingManager Router = new TestRouter();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNextPrevPPTSlide()
        {
            ApplicationReceiver window = new TestWindow()
            {
                Name = "PowerPoint",
                ExecutableName = "powerpnt.exe",
            };

            window.Handle = window.FindWindow();
            Assert.IsTrue(window.Handle != IntPtr.Zero);

            IDeviceSource device = new DeviceSource()
            {
                Handle = (IntPtr)1
            };

            Router.Devices.Add(device);

            Router.Applications.Add(window);

            Router.Routes.Add(new Route()
            {
                Source = device,
                Destination = window,
                InputFilter = new KeyboardRouteInputFilter()
                {
                    Key = Key.PageDown,
                    KeyState = KeyboardRouteInputKeyState.Down
                },
                Actions =
                {
                    new AcceleratorOutputAction()
                    {
                        Accelerator = 393
                    }
                }
            });

            Router.Routes.Add(new Route()
            {
                Source = device,
                Destination = window,
                InputFilter = new KeyboardRouteInputFilter()
                {
                    Key = Key.PageUp,
                    KeyState = KeyboardRouteInputKeyState.Down
                },
                Actions =
                {
                    new AcceleratorOutputAction()
                    {
                        Accelerator = 394
                    }
                }
            });

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = KeyInterop.VirtualKeyFromKey(Key.PageDown),
                IsKeyDown = true
            });

            Thread.Sleep(100);

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = KeyInterop.VirtualKeyFromKey(Key.PageDown),
                IsKeyDown = false
            });

            Thread.Sleep(500);

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = KeyInterop.VirtualKeyFromKey(Key.PageUp),
                IsKeyDown = true
            });

            Thread.Sleep(500);

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = KeyInterop.VirtualKeyFromKey(Key.PageUp),
                IsKeyDown = false
            });

            Assert.Pass();
        }
    }
}