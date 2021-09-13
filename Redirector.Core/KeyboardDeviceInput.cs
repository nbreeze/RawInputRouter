using System;

namespace Redirector.Core
{
    public class KeyboardDeviceInput : DeviceInput
    {
        public int VKey = 0;
        public int ScanCode = 0;
        public bool Extended = false;
        public bool IsKeyDown = false;

        public override bool Matches(DeviceInput otherInput)
        {
            KeyboardDeviceInput otherKb = otherInput as KeyboardDeviceInput;
            if (otherKb == null)
                return false;

            return VKey == otherKb.VKey && Math.Abs(Time - otherKb.Time) < 1000 && otherKb.IsKeyDown == IsKeyDown &&
                otherKb.ScanCode == ScanCode && otherKb.Extended == Extended;
        }

        public override string ToString()
        {
            return "VKey = " + VKey + ", IsKeyDown = " + IsKeyDown + ", ScanCode = " + ScanCode + ", Extended = " + Extended;
        }
    }
}
