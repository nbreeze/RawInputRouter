using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows.Actions
{
    public class KeyboardOutputAction : OutputAction
    {
        private int _VKey = 0;
        public int VKey { get => _VKey; set => SetProperty(ref _VKey, value); }

        public override void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver _destination)
        {
            if (_destination is not Win32ApplicationReceiver destination)
                return;

            User32.PostMessage(destination.Handle, User32.WindowMessage.WM_KEYDOWN, (IntPtr)VKey, IntPtr.Zero);
            User32.PostMessage(destination.Handle, User32.WindowMessage.WM_KEYUP, (IntPtr)VKey, (IntPtr)(3 << 30));
        }
    }
}
