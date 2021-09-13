using Redirector.Core;
using System.Windows.Input;

namespace RawInputRouter
{
    public enum KeyboardRouteInputKeyState
    {
        All,
        Down,
        Up
    }

    public class KeyboardRouteTrigger : RouteTrigger
    {
        private Key? _Key = null;

        public Key? Key { get => _Key; set => SetProperty(ref _Key, value); }

        private KeyboardRouteInputKeyState _KeyState = KeyboardRouteInputKeyState.All;

        public KeyboardRouteInputKeyState KeyState { get => _KeyState; set => SetProperty(ref _KeyState, value); }

        public override bool ShouldTrigger(IRoute route, IDeviceSource source, DeviceInput input)
        {
            if (Key == null)
                return true;

            KeyboardDeviceInput kbInput = input as KeyboardDeviceInput;

            if (KeyState != KeyboardRouteInputKeyState.All)
            {
                if (KeyState == KeyboardRouteInputKeyState.Down && !kbInput.IsKeyDown)
                    return false;
                if (KeyState == KeyboardRouteInputKeyState.Up && kbInput.IsKeyDown)
                    return false;
            }

            if (kbInput == null || KeyInterop.KeyFromVirtualKey(kbInput.VKey) != Key)
                return false;

            return base.ShouldTrigger(route, source, input);
        }
    }
}
