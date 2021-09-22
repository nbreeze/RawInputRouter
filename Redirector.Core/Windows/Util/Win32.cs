using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows.Util
{
    public static class Win32
    {
        [DllImport("user32.dll")]
        private static extern int GetKeyNameText(int lParam, [Out] char[] lpString, int nSize);

        public static string GetVirtualKeyName(int vKey)
        {
            int scanCode = User32.MapVirtualKey(vKey, User32.MapVirtualKeyTranslation.MAPVK_VK_TO_VSC);
            int lParam = scanCode << 16;

            char[] _keyName = new char[64];
            int len = GetKeyNameText(lParam, _keyName, _keyName.Length);
            return new string(_keyName, 0, len);
        }
    }
}
