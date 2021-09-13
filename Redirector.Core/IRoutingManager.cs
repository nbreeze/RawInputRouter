using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Redirector.Core
{
    public interface IRoutingManager : INotifyPropertyChanged
    {
        public ObservableCollection<IApplicationReceiver> Applications { get; }

        public ObservableCollection<IDeviceSource> Devices { get; }

        public ObservableCollection<IRoute> Routes { get; }

        public void OnInput(IDeviceSource source, DeviceInput input);

        public bool ShouldBlockOriginalInput(IDeviceSource source, DeviceInput input);

        public void ProcessRawInputDeviceChangedMessage(IntPtr wParam, IntPtr lParam);

        public bool ProcessRawInputMessage(IntPtr wParam, IntPtr lParam);

        public void ProcessWindowMessage(IntPtr wParam, IntPtr lParam);

        public bool ProcessKeyboardInterceptMessage(IntPtr wParam, IntPtr lParam);
    }
}