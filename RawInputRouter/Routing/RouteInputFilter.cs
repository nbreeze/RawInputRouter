using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace RawInputRouter.Routing
{
    public abstract class RouteInputFilter : ObservableObject, IRouteInputFilter
    {
        public virtual bool PassesFilter(IRoute route, IDeviceSource source, DeviceInput input)
        {
            IDeviceSource routeSource = route.Source;
            return routeSource != null && routeSource.Handle != IntPtr.Zero && routeSource == source;
        }
    }
}
