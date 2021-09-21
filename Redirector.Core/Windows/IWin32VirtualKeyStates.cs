namespace Redirector.Core.Windows
{
    public interface IWin32VirtualKeyStates
    {
        void ClearVirtualKeyStates();
        bool GetVirtualKeyState(Win32KeyboardDeviceInput input);
        void SetVirtualKeyState(Win32KeyboardDeviceInput input, bool state);
    }
}