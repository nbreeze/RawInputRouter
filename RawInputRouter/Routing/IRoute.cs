using RawInputRouter.Imports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RawInputRouter.Routing
{
    public interface IRoute : INotifyPropertyChanged
    {
        public IDeviceSource Source { get; set; }

        public IApplicationReceiver Destination { get; set; }

        public ObservableCollection<IOutputAction> Actions { get; }

        public IRouteInputFilter InputFilter { get; set; }

        public void OnInput(IDeviceSource source, DeviceInput input);

        public bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input);
    }
}
