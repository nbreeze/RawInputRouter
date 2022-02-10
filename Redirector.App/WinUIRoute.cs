using Redirector.App.Serialization;
using Redirector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Redirector.App
{
    [JsonConverter(typeof(WinUIRouteJsonConverter))]
    public class WinUIRoute : Route
    {
        public WinUIRoute() : base()
        {
        }

        public WinUIRoute(WinUIRoute source) : this()
        {
            Copy(source);
        }

        public void Copy(WinUIRoute source)
        {
            Source = source.Source;
            Destination = source.Destination;
            BlockOriginalInput = source.BlockOriginalInput;

            Triggers.Clear();
            foreach (IRouteTrigger trigger in source.Triggers)
            {
                Triggers.Add(trigger);
            }

            Actions.Clear();
            foreach (IOutputAction action in source.Actions)
            {
                Actions.Add(action);
            }
        }
    }
}
