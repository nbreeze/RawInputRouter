using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core
{
    public interface IOutputAction
    {
        public void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination);
    }
}
