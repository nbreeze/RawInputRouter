using RawInputRouter.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
