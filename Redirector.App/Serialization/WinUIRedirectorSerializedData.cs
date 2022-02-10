using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.App.Serialization
{
    public class WinUIRedirectorSerializedData
    {
        public virtual List<WinUIDeviceSource> Devices { get; } = new();

        public virtual List<WinUIApplicationReceiver> Applications { get; } = new();

        public virtual List<WinUIRoute> Routes { get; } = new();

        public WinUIRedirectorSerializedData()
        {
        }

        public WinUIRedirectorSerializedData(IEnumerable<IDeviceSource> sources, IEnumerable<IApplicationReceiver> applications, IEnumerable<IRoute> routes)
            : this()
        {
            if (sources != null)
                Devices.AddRange(sources.Select(source => source as WinUIDeviceSource));

            if (applications != null)
                Applications.AddRange(applications.Select(app => app as WinUIApplicationReceiver));

            if (routes != null)
                Routes.AddRange(routes.Select(route => route as WinUIRoute));
        }
    }
}
