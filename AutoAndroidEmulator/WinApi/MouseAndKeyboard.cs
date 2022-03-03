using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AutoAndroidEmulator.WinApi
{
    public static class MouseAndKeyboard
    {
        /// API Postmessage
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        private static IntPtr CreateLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_MOUSEMOVE = 0x0200;
        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x101;
        const uint WM_MOUSEACTIVATE = 0x0021;
        const uint MK_LBUTTON = 0x0001;
        const uint WM_SETCURSOR = 0x0020;
        const int WM_CHAR = 0x0102;
        public static int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

        public static void MouseUp(IntPtr ptr, int x, int y)
        {
            PostMessage(ptr, WM_LBUTTONUP, 0, MakeLParam(x, y));
        }

        public static void MouseDown(IntPtr ptr, int x, int y)
        {
            PostMessage(ptr, WM_LBUTTONDOWN, (int)MK_LBUTTON, MakeLParam(x, y));
        }

        public static void MouseMove(IntPtr ptr, int x, int y)
        {
            PostMessage(ptr, WM_MOUSEMOVE, 0, MakeLParam(x, y));
        }


        public static void MouseScroll(IntPtr ptr, int x1, int y1, int x2, int y2, int delay = 20)
        {
            MouseDown(ptr, x1, y1);
            Thread.Sleep(delay);
            MouseMove(ptr, x2, y2);
            Thread.Sleep(delay);
            MouseUp(ptr, x2, y2);
        }

        public static void Click(IntPtr ptr, int x, int y, int delay = 5)
        {

            MouseDown(ptr, x, y);
            Thread.Sleep(delay);
            MouseUp(ptr, x, y);
        }

        public static void SendKey(IntPtr ptr, string text, int delay = 10)
        {
            foreach (char c in text.ToUpper())
            {
                PostMessage(ptr, WM_KEYDOWN, (int)c, 0);
                Thread.Sleep(5);
                PostMessage(ptr, WM_KEYUP, (int)c, 0);

                Thread.Sleep(delay);
            }
        }

        public static void SendKeyEmail(IntPtr ptr, string text, int delay = 10)
        {
            foreach (char c in text.ToUpper())
            {
                // 16
                // PostMessage(ptr, WM_CHAR, (int)c, 0);
                if (c == '@')
                {
                    PostMessage(ptr, WM_KEYDOWN, 16, 0);
                    Thread.Sleep(1);
                    PostMessage(ptr, WM_KEYDOWN, 50, 0);
                    Thread.Sleep(1);
                    PostMessage(ptr, WM_KEYUP, 50, 0);
                    Thread.Sleep(1);
                    PostMessage(ptr, WM_KEYUP, 16, 0);
                    Thread.Sleep(delay);
                    continue;
                }
                PostMessage(ptr, WM_KEYDOWN, (int)c, 0);
                Thread.Sleep(2);
                PostMessage(ptr, WM_KEYUP, (int)c, 0);
                Thread.Sleep(delay);
            }
        }

        public static void SendKey(IntPtr ptr, ConsoleKey key, int delay = 5)
        { 
            // PostMessage(ptr, WM_CHAR, keys, 0);
            PostMessage(ptr, WM_KEYDOWN, (int)key, 0);
            Thread.Sleep(delay);
            PostMessage(ptr, WM_KEYUP, (int)key, 0);
        }
    }
}
