using System.ComponentModel;

namespace RawInputRouter.Routing
{
    public interface IRouteInputFilter : INotifyPropertyChanged
    {
        public bool PassesFilter(IRoute route, IDeviceSource source, DeviceInput input);
    }
}
