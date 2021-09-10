using RawInputRouter.Imports;
using System.Collections.Generic;

namespace RawInputRouter.Routing
{
    public sealed class DeviceSourceState
    {
        private IDictionary<int, bool> VirtualKeyStates = new Dictionary<int, bool>();

        private int GetVirtualKeyHash(KeyboardDeviceInput input)
        {
            return input.VKey | (input.ScanCode << 8) | (input.Extended ? (1 << 16) : 0);
        }

        public void ClearVirtualKeyStates()
        {
            VirtualKeyStates.Clear();
        }

        public bool GetVirtualKeyState(KeyboardDeviceInput input)
        {
            VirtualKeyStates.TryGetValue(GetVirtualKeyHash(input), out bool keyDown);
            return keyDown;
        }

        public void SetVirtualKeyState(KeyboardDeviceInput input, bool state)
        {
            VirtualKeyStates[GetVirtualKeyHash(input)] = state;
        }
    }
}
