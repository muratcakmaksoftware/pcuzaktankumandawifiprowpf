using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace wifiuzaktankontrolpro
{
    public class Win32
    {
        
        //cursor set
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int X, int Y);

        //cursor get
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(ref POINT pt);


        //KAYNAK: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mouse_event
        public const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        public const UInt32 MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const UInt32 MOUSEEVENTF_RIGHTUP = 0x0010;
        [DllImport("user32.dll")]
        public static extern void mouse_event(
               UInt32 dwFlags, // motion and click options
               UInt32 dx, // horizontal position or change
               UInt32 dy, // vertical position or change
               UInt32 dwData, // wheel movement
               IntPtr dwExtraInfo // application-defined information
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public static Point GetMousePosition()
        {
            POINT w32Mouse = new POINT();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

    }

}
