using Redirector.App.Serialization;
using Redirector.Core;
using Redirector.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App
{
    [JsonConverter(typeof(WinUIDeviceSourceJsonConverter))]
    public class WinUIDeviceSource : Win32DeviceSource
    {
        public WinUIDeviceSource() : base()
        {
            BlockOriginalInput = true;
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
