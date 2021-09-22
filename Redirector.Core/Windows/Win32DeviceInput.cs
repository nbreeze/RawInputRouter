using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows
{
    public abstract class Win32DeviceInput : DeviceInput
    {
        public IntPtr DeviceHandle = IntPtr.Zero;

        public abstract bool Matches(Win32DeviceInput input);

        public override bool CameFrom(IDeviceSource _source)
        {
            return DeviceHandle != IntPtr.Zero && _source is IWin32DeviceSource source && source.Handle == DeviceHandle;
        }
    }
}
