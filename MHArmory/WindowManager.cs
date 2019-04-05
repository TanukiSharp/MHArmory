using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MHArmory.Configurations;

namespace MHArmory
{
    public interface IWindow
    {
        void OnOpening(bool isAlreadyOpened, object argument);
        void OnOpened(bool isAlreadyOpened, object argument);
    }

    public static class WindowManager
    {
        private class WindowContainer
        {
            public int? Left = null;
            public int? Top = null;
            public int? Width = null;
            public int? Height = null;
            public bool IsMaximized = false;
            public Window Instance = null;
        }

        private static readonly Dictionary<string, WindowContainer> windowContainers = new Dictionary<string, WindowContainer>();

        public static void NotifyConfigurationLoaded()
        {
            Dictionary<string, WindowConfiguration> config = GlobalData.Instance.Configuration.Windows;

            foreach (KeyValuePair<string, WindowConfiguration> item in config)
            {
                windowContainers[item.Key] = new WindowContainer
                {
                    IsMaximized = item.Value.IsMaximized,
                    Left = item.Value.Left,
                    Top = item.Value.Top,
                    Width = item.Value.Width,
                    Height = item.Value.Height,
                };
            }
        }

        public static void SaveWindowsConfiguration()
        {
            Dictionary<string, WindowConfiguration> config = GlobalData.Instance.Configuration.Windows;

            config.Clear();

            foreach (KeyValuePair<string, WindowContainer> item in windowContainers)
            {
                config[item.Key] = new WindowConfiguration
                {
                    IsMaximized = item.Value.IsMaximized,
                    Left = item.Value.Left,
                    Top = item.Value.Top,
                    Width = item.Value.Width,
                    Height = item.Value.Height
                };
            }

            ConfigurationManager.Save(GlobalData.Instance.Configuration);
        }

        public static bool IsInitialized<T>() where T : Window
        {
            return windowContainers.TryGetValue(typeof(T).FullName, out WindowContainer container) && container.Instance != null;
        }

        public static void RestoreWindowState<T>(T instance) where T : Window
        {
            string key = typeof(T).FullName;

            if (windowContainers.TryGetValue(key, out WindowContainer container))
                RestorePositionInternal(container, instance);
        }

        private static WindowContainer RestorePositionInternal<T>() where T : Window
        {
            string key = typeof(T).FullName;

            Window window = null;

            windowContainers.TryGetValue(key, out WindowContainer container);

            if (container == null)
            {
                container = new WindowContainer();
                windowContainers.Add(key, container);
            }

            window = container.Instance;

            if (window == null)
            {
                window = Activator.CreateInstance<T>();
                window.Closing += OnWindowClosing;
                container.Instance = window;
            }

            if (window.IsVisible == false)
                RestorePositionInternal(container, window);
            else if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            return container;
        }

        private static void RestorePositionInternal<T>(WindowContainer container, T window) where T : Window
        {
            if (container.Left.HasValue && container.Top.HasValue && container.Width.HasValue && container.Height.HasValue)
                window.WindowStartupLocation = WindowStartupLocation.Manual;

            if (container.Left.HasValue)
                window.Left = container.Left.Value;
            if (container.Top.HasValue)
                window.Top = container.Top.Value;
            if (container.Width.HasValue)
                window.Width = container.Width.Value;
            if (container.Height.HasValue)
                window.Height = container.Height.Value;

            if (container.IsMaximized)
                window.WindowState = WindowState.Maximized;
        }

        public static void StoreWindowState<T>(T instance) where T : Window
        {
            string key = typeof(T).FullName;

            if (windowContainers.TryGetValue(key, out WindowContainer container) == false)
            {
                container = new WindowContainer();
                windowContainers.Add(key, container);
            }

            StoreInternal(container, instance);
        }

        private static void StoreInternal(WindowContainer container, Window instance)
        {
            if (double.IsInfinity(instance.RestoreBounds.Left) == false)
                container.Left = (int)instance.RestoreBounds.Left;

            if (double.IsInfinity(instance.RestoreBounds.Top) == false)
                container.Top = (int)instance.RestoreBounds.Top;

            if (double.IsInfinity(instance.RestoreBounds.Width) == false)
                container.Width = (int)instance.RestoreBounds.Width;

            if (double.IsInfinity(instance.RestoreBounds.Height) == false)
                container.Height = (int)instance.RestoreBounds.Height;

            container.IsMaximized = instance.WindowState == WindowState.Maximized;
        }

        public static void InitializeWindow<T>(T windowInstance) where T : Window
        {
            if (windowInstance == null)
                throw new ArgumentNullException(nameof(windowInstance));

            string key = typeof(T).FullName;

            if (windowContainers.TryGetValue(key, out WindowContainer container))
            {
                if (container.Instance != null)
                    throw new InvalidOperationException($"Window of type '{key}' is already initialized.");
                else
                    container.Instance = windowInstance;
            }
            else
            {
                container = new WindowContainer { Instance = windowInstance };
                windowContainers.Add(key, container);
            }

            windowInstance.Closing += OnWindowClosing;
        }

        private static void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string key = sender.GetType().FullName;

            WindowContainer container = windowContainers[key];
            Window window = container.Instance;

            StoreInternal(container, window);
        }

        public static void ApplicationClose()
        {
            foreach (KeyValuePair<string, WindowContainer> item in windowContainers)
                item.Value?.Instance?.Close();
        }

        public static bool? ShowDialog<T>(object argument = null) where T : Window
        {
            return ShowInternal<T>(true, argument);
        }

        public static void Show<T>(object argument = null) where T : Window
        {
            ShowInternal<T>(false, argument);
        }

        private static bool? ShowInternal<T>(bool isModal, object argument) where T : Window
        {
            WindowContainer container = RestorePositionInternal<T>();
            Window window = container.Instance;

            bool isAlreadyOpened = window.IsVisible;

            if (window is IWindow win)
                win.OnOpening(isAlreadyOpened, argument);

            FitInScreen(window);

            if (isModal)
                return window.ShowDialog();
            else
            {
                window.Show();
                window.Activate();

                if (window is IWindow win2)
                    win2.OnOpened(isAlreadyOpened, argument);

                return true;
            }
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

            DpiScale dpiInfo = VisualTreeHelper.GetDpi(App.Current.MainWindow);
            monitorInfo.Monitor.bottom = (int)Math.Floor(monitorInfo.Monitor.right / dpiInfo.DpiScaleX);
            monitorInfo.Monitor.bottom = (int)Math.Floor(monitorInfo.Monitor.bottom / dpiInfo.DpiScaleY);
            monitorInfo.WorkArea.right = (int)Math.Floor(monitorInfo.WorkArea.right / dpiInfo.DpiScaleX);
            monitorInfo.WorkArea.bottom = (int)Math.Floor(monitorInfo.WorkArea.bottom / dpiInfo.DpiScaleY);

            int top = (int)window.Top;
            if (top < monitorInfo.WorkArea.top)
            {
                window.Top = monitorInfo.WorkArea.top;
            }

            int bottom = (int)(window.Top + window.Height);
            if (bottom > monitorInfo.WorkArea.bottom)
            {
                if (window.Height < monitorInfo.WorkArea.bottom)
                    window.Top = monitorInfo.WorkArea.bottom - window.Height;
                else
                {
                    window.Top = monitorInfo.WorkArea.top;
                    window.Height = monitorInfo.WorkArea.bottom - monitorInfo.WorkArea.top;
                }
            }

            int left = (int)window.Left;
            if (left < monitorInfo.WorkArea.left)
            {
                window.Left = monitorInfo.WorkArea.left;
            }

            int right = (int)(window.Left + window.Width);
            if (right > monitorInfo.WorkArea.right)
            {
                if (window.Width < monitorInfo.WorkArea.right)
                    window.Left = monitorInfo.WorkArea.right - window.Width;
                else
                {
                    window.Left = monitorInfo.WorkArea.left;
                    window.Width = monitorInfo.WorkArea.right - monitorInfo.WorkArea.left;
                }
            }

            return true;
        }

        #region Native

        private const int MONITOR_DEFAULTTONULL = 0;
        private const int MONITOR_DEFAULTTOPRIMARY = 1;
        private const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        private extern static IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

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

        #endregion // Native
    }
}
