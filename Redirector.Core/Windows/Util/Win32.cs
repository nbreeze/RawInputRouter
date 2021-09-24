using PInvoke;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Redirector.Core.Windows.Util
{
    public static class Win32
    {
        [Flags]
        public enum KeyFlags : int
        {
            /// <summary>
            /// Manipulates the extended key flag.
            /// </summary>
            KF_EXTENDED = 0x0100,
            /// <summary>
            /// Manipulates the dialog mode flag, which indicates whether a dialog box is active.
            /// </summary>
            KF_DLGMODE = 0x0800,
            /// <summary>
            /// Manipulates the menu mode flag, which indicates whether a menu is active.
            /// </summary>
            KF_MENUMODE = 0x1000,
            /// <summary>
            /// Manipulates the ALT key flag, which indicated if the ALT key is pressed.
            /// </summary>
            KF_ALTDOWN = 0x2000,
            /// <summary>
            /// Manipulates the repeat count.
            /// </summary>
            KF_REPEAT = 0x4000,
            /// <summary>
            /// Manipulates the transition state flag.
            /// </summary>
            KF_UP = 0x8000
        }

        [DllImport("user32.dll")]
        private static extern int GetKeyNameText(int lParam, [Out] char[] lpString, int nSize);

        private static readonly User32.VirtualKey[] ExtendedKeys =
        {
            User32.VirtualKey.VK_PRIOR,
            User32.VirtualKey.VK_NEXT,
            User32.VirtualKey.VK_END,
            User32.VirtualKey.VK_HOME,
            User32.VirtualKey.VK_LEFT,
            User32.VirtualKey.VK_UP,
            User32.VirtualKey.VK_RIGHT,
            User32.VirtualKey.VK_DOWN,
            User32.VirtualKey.VK_INSERT,
            User32.VirtualKey.VK_DELETE,
            User32.VirtualKey.VK_LWIN,
            User32.VirtualKey.VK_RWIN,
            User32.VirtualKey.VK_APPS,
            User32.VirtualKey.VK_DIVIDE,
            User32.VirtualKey.VK_NUMLOCK,
            User32.VirtualKey.VK_RCONTROL,
            User32.VirtualKey.VK_RMENU,
        };

        public static string GetVirtualKeyName(int vKey)
        {
            int scanCode = User32.MapVirtualKey(vKey, User32.MapVirtualKeyTranslation.MAPVK_VK_TO_VSC);

            if (Array.BinarySearch(ExtendedKeys, (User32.VirtualKey)vKey) >= 0)
            {
                scanCode |= (int)KeyFlags.KF_EXTENDED;
            }

            int lParam = scanCode << 16;

            char[] _keyName = new char[64];
            int len = GetKeyNameText(lParam, _keyName, _keyName.Length);
            return new string(_keyName, 0, len);
        }
    }
}
