using PInvoke;
using Redirector.Core.Windows.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows.Triggers
{
    public class KeyboardInputRouteTrigger : RouteTrigger
    {
        private int _VKey = 0;
        public int VKey { get => _VKey; set => SetProperty(ref _VKey, value); }

        private bool _KeyDown = true;
        public bool KeyDown { get => _KeyDown; set => SetProperty(ref _KeyDown, value); }

        public override bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput _input)
        {
            if (_input is not Win32KeyboardDeviceInput input)
                return false;

            return input.VKey == VKey && input.IsKeyDown == KeyDown;
        }

        public string GetKeyName() => Win32.GetVirtualKeyName(VKey);
    }
}
