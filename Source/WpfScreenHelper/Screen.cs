using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace WpfScreenHelper
{
    /// <summary>
    /// Represents a display device or multiple display devices on a single system.
    /// </summary>
    public class Screen
    {
        // References:
        // http://referencesource.microsoft.com/#System.Windows.Forms/ndp/fx/src/winforms/Managed/System/WinForms/Screen.cs
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd145072.aspx
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd183314.aspx

        private readonly IntPtr hmonitor;

        // This identifier is just for us, so that we don't try to call the multimon
        // functions if we just need the primary monitor... this is safer for
        // non-multimon OSes.
        private const int PRIMARY_MONITOR = unchecked((int)0xBAADF00D);

        private const int MONITORINFOF_PRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        private static bool multiMonitorSupport;

        static Screen()
        {
            multiMonitorSupport = NativeMethods.GetSystemMetrics(NativeMethods.SM_CMONITORS) != 0;
        }

        private Screen(IntPtr monitor)
            : this(monitor, IntPtr.Zero)
        {
        }

        private Screen(IntPtr monitor, IntPtr hdc)
        {
            if (!multiMonitorSupport || monitor == (IntPtr)PRIMARY_MONITOR)
            {
                this.Bounds = SystemInformation.VirtualScreen;
                this.Primary = true;
                this.DeviceName = "DISPLAY";
            }
            else
            {
                var info = new NativeMethods.MONITORINFOEX();

                NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);

                this.Bounds = new Rect(
                    info.rcMonitor.left, info.rcMonitor.top,
                    info.rcMonitor.right - info.rcMonitor.left,
                    info.rcMonitor.bottom - info.rcMonitor.top);

                this.Primary = ((info.dwFlags & MONITORINFOF_PRIMARY) != 0);

                this.DeviceName = new string(info.szDevice).TrimEnd((char)0);
            }
            hmonitor = monitor;
        }

        /// <summary>
        /// Gets an array of all displays on the system.
        /// </summary>
        /// <returns>An enumerable of type Screen, containing all displays on the system.</returns>
        public static IEnumerable<Screen> AllScreens
        {
            get
            {
                if (multiMonitorSupport)
                {
                    var closure = new MonitorEnumCallback();
                    var proc = new NativeMethods.MonitorEnumProc(closure.Callback);
                    NativeMethods.EnumDisplayMonitors(NativeMethods.NullHandleRef, null, proc, IntPtr.Zero);
                    if (closure.Screens.Count > 0)
                    {
                        return closure.Screens.Cast<Screen>();
                    }
                }
                return new[] { new Screen((IntPtr)PRIMARY_MONITOR) };
            }
        }

        /// <summary>
        /// Gets the bounds of the display.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the bounds of the display.</returns>
        public Rect Bounds { get; private set; }

        /// <summary>
        /// Gets the device name associated with a display.
        /// </summary>
        /// <returns>The device name associated with a display.</returns>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a particular display is the primary device.
        /// </summary>
        /// <returns>true if this display is primary; otherwise, false.</returns>
        public bool Primary { get; private set; }

        /// <summary>
        /// Gets the primary display.
        /// </summary>
        /// <returns>The primary display.</returns>
        public static Screen PrimaryScreen
        {
            get
            {
                if (multiMonitorSupport)
                {
                    return AllScreens.FirstOrDefault(t => t.Primary);
                }
                return new Screen((IntPtr)PRIMARY_MONITOR);
            }
        }

        /// <summary>
        /// Gets the working area of the display. The working area is the desktop area of the display, excluding taskbars, docked windows, and docked tool bars.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the working area of the display.</returns>
        public Rect WorkingArea
        {
            get
            {
                if (!multiMonitorSupport || hmonitor == (IntPtr)PRIMARY_MONITOR)
                {
                    return SystemInformation.WorkingArea;
                }
                var info = new NativeMethods.MONITORINFOEX();
                NativeMethods.GetMonitorInfo(new HandleRef(null, hmonitor), info);
                return new Rect(
                    info.rcWork.left, info.rcWork.top,
                    info.rcWork.right - info.rcWork.left,
                    info.rcWork.bottom - info.rcWork.top);
            }
        }

        /// <summary>
        /// Retrieves a Screen for the display that contains the largest portion of the specified control.
        /// </summary>
        /// <param name="hwnd">The window handle for which to retrieve the Screen.</param>
        /// <returns>A Screen for the display that contains the largest region of the object. In multiple display environments where no display contains any portion of the specified window, the display closest to the object is returned.</returns>
        public static Screen FromHandle(IntPtr hwnd)
        {
            if (multiMonitorSupport)
            {
                return new Screen(NativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2));
            }
            return new Screen((IntPtr)PRIMARY_MONITOR);
        }

        /// <summary>
        /// Retrieves a Screen for the display that contains the specified point.
        /// </summary>
        /// <param name="point">A <see cref="T:System.Windows.Point" /> that specifies the location for which to retrieve a Screen.</param>
        /// <returns>A Screen for the display that contains the point. In multiple display environments where no display contains the point, the display closest to the specified point is returned.</returns>
        public static Screen FromPoint(Point point)
        {
            if (multiMonitorSupport)
            {
                var pt = new NativeMethods.POINTSTRUCT((int)point.X, (int)point.Y);
                return new Screen(NativeMethods.MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST));
            }
            return new Screen((IntPtr)PRIMARY_MONITOR);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the specified object is equal to this Screen.
        /// </summary>
        /// <param name="obj">The object to compare to this Screen.</param>
        /// <returns>true if the specified object is equal to this Screen; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var monitor = obj as Screen;
            if (monitor != null)
            {
                if (hmonitor == monitor.hmonitor)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Computes and retrieves a hash code for an object.
        /// </summary>
        /// <returns>A hash code for an object.</returns>
        public override int GetHashCode()
        {
            return (int)hmonitor;
        }

        private class MonitorEnumCallback
        {
            public ArrayList Screens { get; private set; }

            public MonitorEnumCallback()
            {
                this.Screens = new ArrayList();
            }

            public bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
            {
                this.Screens.Add(new Screen(monitor, hdc));
                return true;
            }
        }
    }
}