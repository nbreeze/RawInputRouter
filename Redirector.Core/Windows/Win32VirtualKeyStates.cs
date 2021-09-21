using System.Collections.Generic;

namespace Redirector.Core.Windows
{
    public sealed class Win32VirtualKeyStates : IWin32VirtualKeyStates
    {
        private Dictionary<int, bool> VirtualKeyStates = new Dictionary<int, bool>();

        private int GetVirtualKeyHash(Win32KeyboardDeviceInput input)
        {
            return input.VKey | input.ScanCode << 8 | (input.Extended ? 1 << 16 : 0);
        }

        public void ClearVirtualKeyStates()
        {
            VirtualKeyStates.Clear();
        }

        public bool GetVirtualKeyState(Win32KeyboardDeviceInput input)
        {
            VirtualKeyStates.TryGetValue(GetVirtualKeyHash(input), out bool keyDown);
            return keyDown;
        }

        public void SetVirtualKeyState(Win32KeyboardDeviceInput input, bool state)
        {
            VirtualKeyStates[GetVirtualKeyHash(input)] = state;
        }
    }
}
