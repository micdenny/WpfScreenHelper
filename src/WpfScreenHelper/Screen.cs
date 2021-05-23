using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace WpfScreenHelper
{
    /// <summary>
    ///     Represents a display device or multiple display devices on a single system.
    /// </summary>
    public class Screen
    {
        // References:
        // http://referencesource.microsoft.com/#System.Windows.Forms/ndp/fx/src/winforms/Managed/System/WinForms/Screen.cs
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd145072.aspx
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd183314.aspx

        // This identifier is just for us, so that we don't try to call the multimon
        // functions if we just need the primary monitor... this is safer for
        // non-multimon OSes.
        private const int PRIMARY_MONITOR = unchecked((int) 0xBAADF00D);

        private const int MONITORINFOF_PRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        private static readonly bool multiMonitorSupport;
        internal static readonly bool PerMonitorDpiAware;

        private readonly IntPtr hmonitor;

        static Screen()
        {
            multiMonitorSupport = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CMONITORS) != 0;
            PerMonitorDpiAware = IsPerMonitorAware();
        }

        private Screen(IntPtr monitor)
            : this(monitor, IntPtr.Zero)
        {
        }

        private Screen(IntPtr monitor, IntPtr hdc)
        {
            if (PerMonitorDpiAware)
            {
                NativeMethods.GetDpiForMonitor(monitor, NativeMethods.DpiType.EFFECTIVE, out var dpiX, out _);

                ScaleFactor = dpiX / 96.0;
            }

            if (!multiMonitorSupport || monitor == (IntPtr) PRIMARY_MONITOR)
            {
                var size = new Size(NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSCREEN),
                    NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CYSCREEN));

                PixelBounds = new Rect(0, 0, size.Width, size.Height);
                Primary = true;
                DeviceName = "DISPLAY";
            }
            else
            {
                var info = new NativeMethods.MONITORINFOEX();

                NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);
                PixelBounds = new Rect(
                    info.rcMonitor.left, info.rcMonitor.top,
                    info.rcMonitor.right - info.rcMonitor.left,
                    info.rcMonitor.bottom - info.rcMonitor.top);
                Primary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;
                DeviceName = new string(info.szDevice).TrimEnd((char) 0);
            }

            hmonitor = monitor;
        }

        /// <summary>
        ///     Gets an array of all displays on the system.
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
                    if (closure.Screens.Count > 0) return closure.Screens.Cast<Screen>();
                }

                return new[] {new Screen((IntPtr) PRIMARY_MONITOR)};
            }
        }

        /// <summary>
        ///     Gets the primary display.
        /// </summary>
        /// <returns>The primary display.</returns>
        public static Screen PrimaryScreen
        {
            get
            {
                return multiMonitorSupport
                    ? AllScreens.FirstOrDefault(t => t.Primary)
                    : new Screen((IntPtr) PRIMARY_MONITOR);
            }
        }

        /// <summary>
        ///     Gets the bounds of the display in pixels.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the bounds of the display in pixels.</returns>
        public Rect PixelBounds { get; }

        /// <summary>
        ///     Gets the bounds of the display in units.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the bounds of the display in units.</returns>
        public Rect Bounds => ScaleFactor.Equals(1.0)
            ? PixelBounds
            : new Rect(PixelBounds.X / ScaleFactor, PixelBounds.Y / ScaleFactor, PixelBounds.Width / ScaleFactor,
                PixelBounds.Height / ScaleFactor);

        /// <summary>
        ///     Gets the device name associated with a display.
        /// </summary>
        /// <returns>The device name associated with a display.</returns>
        public string DeviceName { get; }

        /// <summary>
        ///     Gets a value indicating whether a particular display is the primary device.
        /// </summary>
        /// <returns>true if this display is primary; otherwise, false.</returns>
        public bool Primary { get; }

        /// <summary>
        ///     Gets the scale factor of the display.
        /// </summary>
        /// <returns>The scale factor of the display.</returns>
        public double ScaleFactor { get; } = 1.0;

        /// <summary>
        ///     Gets the working area of the display. The working area is the desktop area of the display, excluding taskbars,
        ///     docked windows, and docked tool bars in units.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the working area of the display in units.</returns>
        public Rect WorkingArea
        {
            get
            {
                Rect workingArea;

                if (!multiMonitorSupport || hmonitor == (IntPtr) PRIMARY_MONITOR)
                {
                    var rc = new NativeMethods.RECT();
                    NativeMethods.SystemParametersInfo(NativeMethods.SPI_GETWORKAREA, 0, ref rc, 0);

                    workingArea = ScaleFactor.Equals(1.0)
                        ? new Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top)
                        : new Rect(rc.left / ScaleFactor, rc.top / ScaleFactor, (rc.right - rc.left) / ScaleFactor,
                            (rc.bottom - rc.top) / ScaleFactor);
                }
                else
                {
                    var info = new NativeMethods.MONITORINFOEX();
                    NativeMethods.GetMonitorInfo(new HandleRef(null, hmonitor), info);

                    workingArea = ScaleFactor.Equals(1.0)
                        ? new Rect(info.rcWork.left, info.rcWork.top, info.rcWork.right - info.rcWork.left,
                            info.rcWork.bottom - info.rcWork.top)
                        : new Rect(info.rcWork.left / ScaleFactor, info.rcWork.top / ScaleFactor,
                            (info.rcWork.right - info.rcWork.left) / ScaleFactor,
                            (info.rcWork.bottom - info.rcWork.top) / ScaleFactor);
                }

                return workingArea;
            }
        }

        /// <summary>
        ///     Retrieves a Screen for the display that contains the largest portion of the specified control.
        /// </summary>
        /// <param name="hwnd">The window handle for which to retrieve the Screen.</param>
        /// <returns>
        ///     A Screen for the display that contains the largest region of the object. In multiple display environments
        ///     where no display contains any portion of the specified window, the display closest to the object is returned.
        /// </returns>
        public static Screen FromHandle(IntPtr hwnd)
        {
            if (multiMonitorSupport) return new Screen(NativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2));

            return new Screen((IntPtr) PRIMARY_MONITOR);
        }

        /// <summary>
        ///     Retrieves a Screen for the display that contains the specified point.
        /// </summary>
        /// <param name="point">A <see cref="T:System.Windows.Point" /> that specifies the location for which to retrieve a Screen.</param>
        /// <returns>
        ///     A Screen for the display that contains the point. In multiple display environments where no display contains
        ///     the point, the display closest to the specified point is returned.
        /// </returns>
        public static Screen FromPoint(Point point)
        {
            if (multiMonitorSupport)
            {
                var pt = new NativeMethods.POINTSTRUCT((int) point.X, (int) point.Y);
                return new Screen(NativeMethods.MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST));
            }

            return new Screen((IntPtr) PRIMARY_MONITOR);
        }

        /// <summary>
        ///     Retrieves a Screen for the display that contains the specified point.
        /// </summary>
        /// <param name="point">
        ///     A <see cref="T:NativeMethods.POINTSTRUCT" /> that specifies the location for which to retrieve a
        ///     Screen.
        /// </param>
        /// <returns>
        ///     A Screen for the display that contains the point. In multiple display environments where no display contains
        ///     the point, the display closest to the specified point is returned.
        /// </returns>
        internal static Screen FromPoint(NativeMethods.POINTSTRUCT point)
        {
            if (multiMonitorSupport) return new Screen(NativeMethods.MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST));

            return new Screen((IntPtr) PRIMARY_MONITOR);
        }

        /// <summary>
        ///     Retrieves current process dpi awareness.
        /// </summary>
        /// <returns>true if current process dpi awareness equals per monitor dpi aware.</returns>
        private static bool IsPerMonitorAware()
        {
            NativeMethods.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out var value);

            return value == NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the specified object is equal to this Screen.
        /// </summary>
        /// <param name="obj">The object to compare to this Screen.</param>
        /// <returns>true if the specified object is equal to this Screen; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Screen monitor)
                if (hmonitor == monitor.hmonitor)
                    return true;

            return false;
        }

        /// <summary>
        ///     Computes and retrieves a hash code for an object.
        /// </summary>
        /// <returns>A hash code for an object.</returns>
        public override int GetHashCode()
        {
            return (int) hmonitor;
        }

        private class MonitorEnumCallback
        {
            public MonitorEnumCallback()
            {
                Screens = new ArrayList();
            }

            public ArrayList Screens { get; }

            public bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
            {
                Screens.Add(new Screen(monitor, hdc));
                return true;
            }
        }
    }
}