using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.WinUI
{
    public class WinUIDeviceSource : DeviceSource
    {
        public WinUIDeviceSource() : base()
        {
        }

        public WinUIDeviceSource(WinUIDeviceSource source) : this()
        {
            Copy(source);
        }

        public void Copy(WinUIDeviceSource source)
        {
            Name = source.Name;
            Path = source.Path;
            Handle = source.Handle;
            BlockOriginalInput = source.BlockOriginalInput;
        }

        public string DisplayDeviceName(IntPtr handle, string name)
        {
            if (Handle == IntPtr.Zero)
                return $"<Not connected> {name}";

            return $"[0x{Handle:X}] {name}";
        }

        public override void OnDelete()
        {
            foreach (IRoute route in App.Current.Redirector.Routes)
            {
                if (route.Source == this)
                {
                    route.Source = null;
                }
            }
        }
    }
}
