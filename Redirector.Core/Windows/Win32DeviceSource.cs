using Redirector.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows
{

    public class Win32DeviceSource : DeviceSource, IWin32DeviceSource
    {
        public IWin32VirtualKeyStates VirtualKeyStates { get; } = new Win32VirtualKeyStates();

        private IntPtr _Handle = IntPtr.Zero;
        public IntPtr Handle { get => _Handle; set => SetProperty(ref _Handle, value); }

        public virtual IntPtr FindHandle()
        {
            List<RawInput.RAWINPUTDEVICELIST> devices = new();
            RawInput.GetRawInputDeviceList(devices);

            foreach (RawInput.RAWINPUTDEVICELIST device in devices)
            {
                string deviceName = RawInput.GetRawInputDeviceInterfaceName(device.hDevice);
                if (deviceName.Equals(Path))
                {
                    return device.hDevice;
                }
            }

            return IntPtr.Zero;
        }
        public override void OnInput(DeviceInput input)
        {
            if (input is Win32KeyboardDeviceInput kbInput)
            {
                VirtualKeyStates.SetVirtualKeyState(kbInput, kbInput.IsKeyDown);
            }
        }

        public override void OnConnect()
        {
            VirtualKeyStates.ClearVirtualKeyStates();
        }

        public override void OnDisconnect()
        {
            VirtualKeyStates.ClearVirtualKeyStates();
        }
    }
}
