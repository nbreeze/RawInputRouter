using System.Windows.Input;

namespace RawInputRouter.Routing
{
    public enum KeyboardRouteInputKeyState
    {
        All,
        Down,
        Up
    }

    public class KeyboardRouteInputFilter : RouteInputFilter
    {
        private Key? _Key = null;

        public Key? Key { get => _Key; set => SetProperty(ref _Key, value); }

        private KeyboardRouteInputKeyState _KeyState = KeyboardRouteInputKeyState.All;

        public KeyboardRouteInputKeyState KeyState { get => _KeyState; set => SetProperty(ref _KeyState, value); }

        public override bool PassesFilter(IRoute route, IDeviceSource source, DeviceInput input)
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

            return base.PassesFilter(route, source, input);
        }
    }
}
