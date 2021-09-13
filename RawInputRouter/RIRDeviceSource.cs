using Redirector.Core;

namespace RawInputRouter
{
    public class RIRDeviceSource : DeviceSource
    {
        public override bool ShouldBlockOriginalInput(DeviceInput input)
        {
            return base.ShouldBlockOriginalInput(input);
        }
    }
}
