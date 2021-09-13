using NUnit.Framework;
using Redirector.Core;
using System;
using System.IO;
using System.Threading;
using PInvoke;
using System.Linq;

namespace UnitTests
{
    public class TestPPTRouting
    {
        private class TestRouter : RoutingManager
        {
        }

        private class TestWindow : ApplicationReceiver
        {
            public override bool IsMatchingWindow(IntPtr handle)
            {
                char[] windowTitle = new char[260];
                User32.GetWindowText(handle, windowTitle, windowTitle.Length);

                string str = new string(windowTitle).Replace("\0", "");
                return str.Contains("Presenter View");
            }
        }

        private class TestKeyboardRouteTrigger : RouteTrigger
        {
            public User32.VirtualKey Key = 0;
            public bool KeyDown = false;

            public override bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput input)
            {
                KeyboardDeviceInput kbInput = input as KeyboardDeviceInput;
                if (kbInput == null)
                    return false;

                if (KeyDown != kbInput.IsKeyDown)
                    return false;

                if (kbInput.VKey != (int)Key)
                    return false;

                return base.ShouldTrigger(route, source, input);
            }
        }

        private RoutingManager Router = new TestRouter();

        private Microsoft.Office.Interop.PowerPoint.Application PowerPoint;
        private Microsoft.Office.Interop.PowerPoint.Presentation ppPresentation; 

        [OneTimeSetUp]
        public void Setup()
        {
            string cwd = Directory.GetCurrentDirectory();

            PowerPoint = new();
            PowerPoint.Activate();

            var presentations = PowerPoint.Presentations;
            ppPresentation = presentations.Open(Path.Combine(cwd, "TestPresentation.pptx"));

            ppPresentation.SlideShowSettings.ShowPresenterView = Microsoft.Office.Core.MsoTriState.msoTrue;
            ppPresentation.SlideShowSettings.Run();

            Thread.Sleep(500);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (ppPresentation != null)
            {
                ppPresentation.Close();
            }

            if (PowerPoint != null)
            {
                PowerPoint.Quit();
            }
        }

        [Test]
        public void TestNextPrevPPTSlide()
        {
            ApplicationReceiver window = new TestWindow()
            {
                Name = "PowerPoint",
                ExecutableName = "powerpnt.exe",
            };

            Assert.IsTrue(window.FindProcesses().Any());

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
                Triggers = {
                    new TestKeyboardRouteTrigger()
                    {
                        Key = User32.VirtualKey.VK_NEXT,
                        KeyDown = true
                    } 
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
                Triggers = {
                    new TestKeyboardRouteTrigger()
                    {
                        Key = User32.VirtualKey.VK_PRIOR,
                        KeyDown = true
                    }
                },
                Actions =
                {
                    new AcceleratorOutputAction()
                    {
                        Accelerator = 394
                    }
                }
            });

            int expectedSlideIndex = ppPresentation.SlideShowWindow.View.Slide.SlideIndex + 1;

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = (int)User32.VirtualKey.VK_NEXT,
                IsKeyDown = true
            });

            Thread.Sleep(100);

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = (int)User32.VirtualKey.VK_NEXT,
                IsKeyDown = false
            });

            Thread.Sleep(500);

            Assert.AreEqual(expectedSlideIndex, ppPresentation.SlideShowWindow.View.Slide.SlideIndex);

            expectedSlideIndex = expectedSlideIndex - 1;

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = (int)User32.VirtualKey.VK_PRIOR,
                IsKeyDown = true
            });

            Thread.Sleep(100);

            Router.OnInput(device, new KeyboardDeviceInput()
            {
                DeviceHandle = (IntPtr)1,
                VKey = (int)User32.VirtualKey.VK_PRIOR,
                IsKeyDown = false
            });

            Thread.Sleep(500);

            Assert.AreEqual(expectedSlideIndex, ppPresentation.SlideShowWindow.View.Slide.SlideIndex);
        }
    }
}