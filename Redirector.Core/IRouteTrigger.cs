using System.ComponentModel;

namespace Redirector.Core
{
    public interface IRouteTrigger : INotifyPropertyChanged
    {
        public bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput input);
    }
}
