using CommunityToolkit.Mvvm.ComponentModel;

namespace Redirector.Core
{
    public abstract class OutputAction : ObservableObject, IOutputAction
    {
        public abstract void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination);
    }
}
