using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Redirector.Core
{
    public interface IRedirector : INotifyPropertyChanged
    {
        public ObservableCollection<IApplicationReceiver> Applications { get; }

        public ObservableCollection<IDeviceSource> Devices { get; }

        public ObservableCollection<IRoute> Routes { get; }

        public void OnInput(IDeviceSource source, DeviceInput input);

        public bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input);
    }
}