using System;

namespace Redirector.Core
{
    public abstract class DeviceInput
    {
        public IntPtr DeviceHandle = IntPtr.Zero;
        public int Time = 0;

        /// <summary>
        /// TODO: This is strictly a Win32 problem and should be abstracted away!
        /// </summary>
        /// <param name="otherInput"></param>
        /// <returns></returns>
        public abstract bool Matches(DeviceInput otherInput);
    }
}
