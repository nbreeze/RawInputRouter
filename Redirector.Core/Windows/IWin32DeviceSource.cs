using System;

namespace Redirector.Core.Windows
{
    public interface IWin32DeviceSource : IDeviceSource
    {
        IntPtr Handle { get; set; }
        IWin32VirtualKeyStates VirtualKeyStates { get; }

        IntPtr FindHandle();
    }
}