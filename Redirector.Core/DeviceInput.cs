using System;

namespace Redirector.Core
{
    public abstract class DeviceInput
    {
        public IntPtr DeviceHandle = IntPtr.Zero;
        public int Time = 0;
    }
}
