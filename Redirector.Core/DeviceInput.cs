using System;

namespace Redirector.Core
{
    public abstract class DeviceInput
    {
        public int Time = 0;

        public abstract bool CameFrom(IDeviceSource source);
    }
}
