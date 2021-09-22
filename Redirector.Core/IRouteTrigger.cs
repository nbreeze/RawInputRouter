using System.ComponentModel;

namespace Redirector.Core
{
    public interface IRouteTrigger : INotifyPropertyChanged
    {
        public IRoute Route { get; set; }

        public bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput input);
    }
}
