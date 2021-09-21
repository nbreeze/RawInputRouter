using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core
{
    public interface IDeviceSource : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public bool BlockOriginalInput { get; set; }

        public bool ShouldBlockOriginalInput(DeviceInput input);

        public void OnInput(DeviceInput input);

        public void OnConnect();

        public void OnDisconnect();

        public void OnDelete();
    }
}
