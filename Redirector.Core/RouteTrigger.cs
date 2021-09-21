using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Redirector.Core
{
    public abstract class RouteTrigger : ObservableObject, IRouteTrigger
    {
        public virtual bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput input)
        {
            IDeviceSource routeSource = route.Source;
            return routeSource != null && routeSource == source;
        }
    }
}
