using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInputRouter.Routing
{
    public interface IOutputAction
    {
        public void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination);
    }
}
