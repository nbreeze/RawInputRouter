using RawInputRouter.Imports;
using System;

namespace RawInputRouter.Routing
{
    public class AcceleratorOutputAction : OutputAction
    {
        private int _Accelerator = 0;

        public int Accelerator { get => _Accelerator; set => SetProperty(ref _Accelerator, value); }

        public override void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination)
        {
            if (destination == null || destination.Handle == IntPtr.Zero)
                return;

            User32.PostMessage(destination.Handle, User32.WM_COMMAND, (IntPtr)((1 << 16) | Accelerator), IntPtr.Zero);
        }
    }
}
