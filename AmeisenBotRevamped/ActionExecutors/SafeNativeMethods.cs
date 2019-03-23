using System;
using System.Runtime.InteropServices;

namespace AmeisenBotRevamped.ActionExecutors
{
    internal class SafeNativeMethods
    {
        public struct Rect
        {
            public int Bottom { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
            public int Top { get; set; }
        }

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}