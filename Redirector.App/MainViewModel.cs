using CommunityToolkit.Mvvm.ComponentModel;
using PInvoke;
using Redirector.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.App
{
    public class CaptureDeviceEventArgs : EventArgs
    {
        public RawInput.RAWINPUT RawInput { get; private set; }

        public CaptureDeviceEventArgs(ref RawInput.RAWINPUT input)
        {
            RawInput = input;
        }
    }

    public class MainViewModel : ObservableObject
    {
        private string _TopLevelHeader = "";

        public string TopLevelHeader { get => _TopLevelHeader; set => SetProperty(ref _TopLevelHeader, value); }

        private bool _IsCapturingDevice = false;

        public bool IsCapturingDevice { get => _IsCapturingDevice; set => SetProperty(ref _IsCapturingDevice, value); }

        public event EventHandler<CaptureDeviceEventArgs> CaptureDevice;

        public void InvokeCaptureDeviceEvent(object sender, CaptureDeviceEventArgs eventArgs)
        {
            CaptureDevice?.Invoke(sender, eventArgs);
        }
    }
}
