using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core
{
    public interface IOutputAction : INotifyPropertyChanged
    {
        public IRoute Route { get; set; }

        public void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination);
    }
}
