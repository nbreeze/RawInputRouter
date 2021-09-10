using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawInputRouter.Routing
{
    public interface IDeviceSource : INotifyPropertyChanged
    {
        public DeviceSourceState State { get; }

        public string Name { get; set; }

        public string Path { get; set; }

        public IntPtr Handle { get; set; }

        public bool BlockOriginalInput { get; set; }

        public bool ShouldBlockOriginalInput(DeviceInput input);

        public IntPtr FindHandle();

        public void OnInput(DeviceInput input);

        public void OnConnect();

        public void OnDisconnect();

        public void OnDelete();
    }
}
