using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using PInvoke;
using Redirector.Native;

namespace Redirector.Core
{
    public class DeviceSource : ObservableObject, IDeviceSource
    {
        public DeviceSourceState State { get; } = new();

        private string _Name = "";
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }

        private string _Path = "";
        public string Path { get => _Path; set => SetProperty(ref _Path, value); }

        private IntPtr _Handle = IntPtr.Zero;
        public IntPtr Handle { get => _Handle; set => SetProperty(ref _Handle, value); }

        private bool _Block = false;
        public bool BlockOriginalInput { get => _Block; set => SetProperty(ref _Block, value); }

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

        public virtual void OnInput(DeviceInput input)
        {
            if (input is KeyboardDeviceInput kbInput)
            {
                State.SetVirtualKeyState(kbInput, kbInput.IsKeyDown);
            }
        }

        public virtual void OnConnect()
        {
            State.ClearVirtualKeyStates();
        }

        public virtual void OnDisconnect()
        {
            State.ClearVirtualKeyStates();
        }

        public virtual bool ShouldBlockOriginalInput(DeviceInput input)
        {
            return BlockOriginalInput;
        }

        public virtual void OnDelete() { }
    }
}
