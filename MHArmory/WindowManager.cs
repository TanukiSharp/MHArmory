using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MHArmory
{
    public static class WindowManager
    {
        private const int MONITOR_DEFAULTTONULL = 0;
        private const int MONITOR_DEFAULTTOPRIMARY = 1;
        private const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        // size of a device name string
        private const int CCHDEVICENAME = 32;

        /// <summary>
        /// The MONITORINFOEX structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
        /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name 
        /// for the display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfoEx
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function. 
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public NativeRect Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications, 
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor. 
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public NativeRect WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            /// 
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
            public uint Flags;

            /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name, 
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;

            public static MonitorInfoEx Create()
            {
                return new MonitorInfoEx
                {
                    Size = 40 + 2 * CCHDEVICENAME,
                    Monitor = new NativeRect(),
                    WorkArea = new NativeRect(),
                    DeviceName = string.Empty,
                    Flags = 0
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public static bool FitInScreen(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);

            IntPtr hMonitor = MonitorFromWindow(windowInteropHelper.Handle, MONITOR_DEFAULTTONEAREST);
            if (hMonitor == IntPtr.Zero)
                return false;

            var monitorInfo = MonitorInfoEx.Create();
            if (GetMonitorInfo(hMonitor, ref monitorInfo) == false)
                return false;

            int top = (int)window.Top;
            if (top < monitorInfo.WorkArea.top)
            {
                window.Top = monitorInfo.WorkArea.top;
            }

            int bottom = (int)(window.Top + window.Height);
            if (bottom > monitorInfo.WorkArea.bottom)
            {
                window.Height -= (bottom - monitorInfo.WorkArea.bottom);
            }

            int left = (int)window.Left;
            if (left < monitorInfo.WorkArea.left)
            {
                window.Left = monitorInfo.WorkArea.left;
            }

            int right = (int)(window.Left + window.Width);
            if (right > monitorInfo.WorkArea.right)
            {
                window.Width -= (right - monitorInfo.WorkArea.right);
            }

            return true;
        }
    }
}
