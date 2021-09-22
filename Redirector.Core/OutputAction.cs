using CommunityToolkit.Mvvm.ComponentModel;

namespace Redirector.Core
{
    public abstract class OutputAction : ObservableObject, IOutputAction
    {
        private IRoute _Route = null;
        public IRoute Route { get => _Route; set => SetProperty(ref _Route, value); }

        public abstract void Dispatch(DeviceInput input, IDeviceSource source, IApplicationReceiver destination);
    }
}
