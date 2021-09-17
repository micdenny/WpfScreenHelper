namespace WpfScreenHelper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    ///     Represents a display device or multiple display devices on a single system.
    /// </summary>
    public class Screen
    {
        // References:
        // http://referencesource.microsoft.com/#System.Windows.Forms/ndp/fx/src/winforms/Managed/System/WinForms/Screen.cs
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd145072.aspx
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd183314.aspx

        /// <summary>
        /// Indicates if we have more than one monitor.
        /// </summary>
        private static readonly bool MultiMonitorSupport;

        // This identifier is just for us, so that we don't try to call the multimon
        // functions if we just need the primary monitor... this is safer for
        // non-multimon OSes.
        private const int PRIMARY_MONITOR = unchecked((int)0xBAADF00D);

        private const int MONITORINFOF_PRIMARY = 0x00000001;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        /// <summary>
        /// The monitor handle.
        /// </summary>
        private readonly IntPtr monitorHandle;

        /// <summary>
        /// Initializes static members of the <see cref="Screen"/> class.
        /// </summary>
        static Screen()
        {
            MultiMonitorSupport = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CMONITORS) != 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen"/> class.
        /// </summary>
        /// <param name="monitor">
        /// The monitor.
        /// </param>
        private Screen(IntPtr monitor)
            : this(monitor, IntPtr.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen"/> class.
        /// </summary>
        /// <param name="monitor">
        /// The monitor.
        /// </param>
        /// <param name="hdc">
        /// The hdc.
        /// </param>
        private Screen(IntPtr monitor, IntPtr hdc)
        {
            if (NativeMethods.IsProcessDPIAware())
            {
                NativeMethods.GetDpiForMonitor(monitor, NativeMethods.DpiType.EFFECTIVE, out var dpiX, out _);

                this.ScaleFactor = dpiX / 96.0;
            }

            if (!MultiMonitorSupport || monitor == (IntPtr)PRIMARY_MONITOR)
            {
                var size = new Size(
                    NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSCREEN),
                    NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CYSCREEN));

                this.PixelBounds = new Rect(0, 0, size.Width, size.Height);
                this.Primary = true;
                this.DeviceName = "DISPLAY";
            }
            else
            {
                var info = new NativeMethods.MONITORINFOEX();

                NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);

                this.PixelBounds = new Rect(
                    info.rcMonitor.left,
                    info.rcMonitor.top,
                    info.rcMonitor.right - info.rcMonitor.left,
                    info.rcMonitor.bottom - info.rcMonitor.top);
                this.Primary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;
                this.DeviceName = new string(info.szDevice).TrimEnd((char)0);
            }

            this.monitorHandle = monitor;
        }

        /// <summary>
        ///     Gets an array of all displays on the system.
        /// </summary>
        /// <returns>An enumerable of type Screen, containing all displays on the system.</returns>
        public static IEnumerable<Screen> AllScreens
        {
            get
            {
                if (MultiMonitorSupport)
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
        ///     Gets the primary display.
        /// </summary>
        /// <returns>The primary display.</returns>
        public static Screen PrimaryScreen
        {
            get
            {
                return MultiMonitorSupport ? AllScreens.FirstOrDefault(t => t.Primary) : new Screen((IntPtr)PRIMARY_MONITOR);
            }
        }

        /// <summary>
        ///     Gets the bounds of the display in units.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the bounds of the display in units.</returns>
        public Rect Bounds =>
            this.ScaleFactor.Equals(1.0)
                ? this.PixelBounds
                : new Rect(
                    this.PixelBounds.X / this.ScaleFactor,
                    this.PixelBounds.Y / this.ScaleFactor,
                    this.PixelBounds.Width / this.ScaleFactor,
                    this.PixelBounds.Height / this.ScaleFactor);

        /// <summary>
        ///     Gets the device name associated with a display.
        /// </summary>
        /// <returns>The device name associated with a display.</returns>
        public string DeviceName { get; }

        /// <summary>
        ///     Gets the bounds of the display in pixels.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the bounds of the display in pixels.</returns>
        public Rect PixelBounds { get; }

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
        ///     Gets the working area of the display. The working area is the desktop area of the display, excluding task bars,
        ///     docked windows, and docked tool bars in units.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" />, representing the working area of the display in units.</returns>
        public Rect WorkingArea
        {
            get
            {
                Rect workingArea;

                if (!MultiMonitorSupport || this.monitorHandle == (IntPtr)PRIMARY_MONITOR)
                {
                    var rc = new NativeMethods.RECT();

                    NativeMethods.SystemParametersInfo(NativeMethods.SPI.SPI_GETWORKAREA, 0, ref rc, NativeMethods.SPIF.SPIF_SENDCHANGE);

                    workingArea = this.ScaleFactor.Equals(1.0)
                                      ? new Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top)
                                      : new Rect(
                                          rc.left / this.ScaleFactor,
                                          rc.top / this.ScaleFactor,
                                          (rc.right - rc.left) / this.ScaleFactor,
                                          (rc.bottom - rc.top) / this.ScaleFactor);
                }
                else
                {
                    var info = new NativeMethods.MONITORINFOEX();
                    NativeMethods.GetMonitorInfo(new HandleRef(null, this.monitorHandle), info);

                    workingArea = this.ScaleFactor.Equals(1.0)
                                      ? new Rect(
                                          info.rcWork.left,
                                          info.rcWork.top,
                                          info.rcWork.right - info.rcWork.left,
                                          info.rcWork.bottom - info.rcWork.top)
                                      : new Rect(
                                          info.rcWork.left / this.ScaleFactor,
                                          info.rcWork.top / this.ScaleFactor,
                                          (info.rcWork.right - info.rcWork.left) / this.ScaleFactor,
                                          (info.rcWork.bottom - info.rcWork.top) / this.ScaleFactor);
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
            return MultiMonitorSupport
                       ? new Screen(NativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2))
                       : new Screen((IntPtr)PRIMARY_MONITOR);
        }

        /// <summary>
        ///     Retrieves a Screen for the display that contains the specified point in pixels.
        /// </summary>
        /// <param name="point">A <see cref="T:System.Windows.Point" /> that specifies the location for which to retrieve a Screen.</param>
        /// <returns>
        ///     A Screen for the display that contains the point in pixels. In multiple display environments where no display contains
        ///     the point, the display closest to the specified point is returned.
        /// </returns>
        public static Screen FromPoint(Point point)
        {
            if (MultiMonitorSupport)
            {
                var pt = new NativeMethods.POINTSTRUCT((int)point.X, (int)point.Y);
                return new Screen(NativeMethods.MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST));
            }
            return new Screen((IntPtr)PRIMARY_MONITOR);
        }

        /// <summary>
        ///     Retrieves a Screen for the display that contains the largest portion of the specified control.
        /// </summary>
        /// <param name="window">The window for which to retrieve the Screen.</param>
        /// <returns>
        ///     A Screen for the display that contains the largest region of the object. In multiple display environments
        ///     where no display contains any portion of the specified window, the display closest to the object is returned.
        /// </returns>
        public static Screen FromWindow(Window window)
        {
            return FromHandle(new WindowInteropHelper(window).Handle);
        }

        /// <summary>
        ///     Retrieves a Screen for the display that contains the specified point in units.
        /// </summary>
        /// <param name="point">A <see cref="T:System.Windows.Point" /> that specifies the location for which to retrieve a Screen.</param>
        /// <returns>
        ///     A Screen for the display that contains the point in units. In multiple display environments where no display contains
        ///     the point, the display closest to the specified point is returned.
        /// </returns>
        public static Screen FromWpfPoint(Point point)
        {
            if (MultiMonitorSupport)
            {
                foreach (var screen in AllScreens)
                {
                    if (screen.Bounds.Contains(point))
                    {
                        return screen;
                    }
                }
            }

            return new Screen((IntPtr)PRIMARY_MONITOR);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the specified object is equal to this Screen.
        /// </summary>
        /// <param name="obj">The object to compare to this Screen.</param>
        /// <returns>true if the specified object is equal to this Screen; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Screen monitor)
            {
                if (this.monitorHandle == monitor.monitorHandle)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Computes and retrieves a hash code for an object.
        /// </summary>
        /// <returns>A hash code for an object.</returns>
        public override int GetHashCode()
        {
            return this.monitorHandle.GetHashCode();
        }

        /// <summary>
        /// The monitor enum callback.
        /// </summary>
        private class MonitorEnumCallback
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MonitorEnumCallback"/> class.
            /// </summary>
            public MonitorEnumCallback()
            {
                this.Screens = new ArrayList();
            }

            /// <summary>
            /// Gets the screens.
            /// </summary>
            public ArrayList Screens { get; }

            public bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
            {
                this.Screens.Add(new Screen(monitor, hdc));
                return true;
            }
        }
    }
}