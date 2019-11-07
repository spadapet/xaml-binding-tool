using System;
using System.Runtime.InteropServices;

namespace XamlBinding.Utility
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public static bool FlashWindow(IntPtr hwnd, bool flashCaption, bool flashTray)
        {
            FLASHWINFO info = new FLASHWINFO();
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.hwnd = hwnd;
            info.dwFlags = 12 + (flashCaption ? 1u : 0) + (flashTray ? 2u : 0);
            info.uCount = 4;
            info.dwTimeout = 0;

            return NativeMethods.FlashWindowEx(ref info);
        }
    }
}
