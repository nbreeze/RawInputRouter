using System;

namespace Redirector.Core
{
    public abstract class DeviceInput
    {
        public IntPtr DeviceHandle = IntPtr.Zero;
        public int Time = 0;

        public abstract bool Matches(DeviceInput otherInput);
    }
}
