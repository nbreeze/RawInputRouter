using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Redirector.Core
{
    public abstract class RouteTrigger : ObservableObject, IRouteTrigger
    {
        private IRoute _Route = null;
        public IRoute Route { get => _Route; set => SetProperty(ref _Route, value); }

        public virtual bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput input)
        {
            IDeviceSource routeSource = route.Source;
            return routeSource != null && routeSource == source;
        }
    }
}
