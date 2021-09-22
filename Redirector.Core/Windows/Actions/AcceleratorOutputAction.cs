using System;
using PInvoke;

namespace Redirector.Core.Windows.Actions
{
    public class AcceleratorOutputAction : OutputAction
    {
        private int _Accelerator = 0;

        public int Accelerator { get => _Accelerator; set => SetProperty(ref _Accelerator, value); }

        public override void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver _destination)
        {
            if (_destination is not Win32ApplicationReceiver destination)
                return;

            User32.PostMessage(destination.Handle, User32.WindowMessage.WM_COMMAND, (IntPtr)(1 << 16 | Accelerator), IntPtr.Zero);
        }
    }
}
