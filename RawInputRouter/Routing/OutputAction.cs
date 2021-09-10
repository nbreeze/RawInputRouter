using CommunityToolkit.Mvvm.ComponentModel;

namespace RawInputRouter.Routing
{
    public abstract class OutputAction : ObservableObject, IOutputAction
    {
        public abstract void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination);
    }
}
