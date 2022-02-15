using Redirector.Core.Windows.Triggers;
using Redirector.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Redirector.App.Serialization.Triggers;

namespace Redirector.App.Triggers
{
    [JsonConverter(typeof(WinUIKeyboardInputRouteTriggerJsonConverter))]
    public class WinUIKeyboardInputRouteTrigger : KeyboardInputRouteTrigger, IWinUIRouteTrigger
    {
        public void Copy(IWinUIRouteTrigger _trigger)
        {
            if (_trigger is not WinUIKeyboardInputRouteTrigger trigger)
                return;

            VKey = trigger.VKey;
            KeyDown = trigger.KeyDown;
        }

        public string GetKeyName(int vKey) => GetKeyName();

        public string GetDisplayString(int vKey, bool keyDown)
        {
            string pressorRelease = keyDown ? "PRESSED" : "RELEASED";

            return $"{GetKeyName(vKey)} Key {pressorRelease}";
        }
    }
}
