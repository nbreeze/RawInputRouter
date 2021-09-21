using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows
{
    public class Win32KeyboardDeviceInput : Win32DeviceInput
    {
        public int VKey = 0;
        public int ScanCode = 0;
        public bool Extended = false;
        public bool IsKeyDown = false;

        public override bool Matches(Win32DeviceInput otherInput)
        {
            if (otherInput is not Win32KeyboardDeviceInput otherKb)
                return false;

            return VKey == otherKb.VKey && otherKb.IsKeyDown == IsKeyDown &&
                otherKb.ScanCode == ScanCode && otherKb.Extended == Extended && Math.Abs(Time - otherKb.Time) < 1000;
        }

        public override string ToString()
        {
            return "VKey = " + VKey + ", IsKeyDown = " + IsKeyDown + ", ScanCode = " + ScanCode + ", Extended = " + Extended;
        }
    }
}
