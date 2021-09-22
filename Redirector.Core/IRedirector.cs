using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Redirector.Core
{
    public class RedirectorInputEventArgs : EventArgs
    {
        public IDeviceSource Source { get; private set; }
        public DeviceInput Input { get; private set; }

        public RedirectorInputEventArgs(IDeviceSource source, DeviceInput input)
        {
            Source = source;
            Input = input;
        }
    }

    public interface IRedirector : INotifyPropertyChanged
    {
        public ObservableCollection<IApplicationReceiver> Applications { get; }

        public ObservableCollection<IDeviceSource> Devices { get; }

        public ObservableCollection<IRoute> Routes { get; }

        public event EventHandler<RedirectorInputEventArgs> Input;

        public void OnInput(IDeviceSource source, DeviceInput input);

        public bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input);
    }
}