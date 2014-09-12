using System.Windows;

namespace WpfScreenHelper
{
    public static class SystemInformation
    {
        /// <summary>
        /// Gets the bounds of the virtual screen.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" /> that specifies the bounding rectangle of the entire virtual screen.</returns>
        public static Rect VirtualScreen
        {
            get
            {
                var size = new Size(NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN),
                                    NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN));
                return new Rect(0, 0, size.Width, size.Height);
            }
        }

        /// <summary>
        /// Gets the size, in pixels, of the working area of the screen.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" /> that represents the size, in pixels, of the working area of the screen.</returns>
        public static Rect WorkingArea
        {
            get
            {
                NativeMethods.RECT rc = new NativeMethods.RECT();
                NativeMethods.SystemParametersInfo(NativeMethods.SPI_GETWORKAREA, 0, ref rc, 0);
                return new Rect(rc.left,
                                rc.top,
                                rc.right - rc.left,
                                rc.bottom - rc.top);
            }
        }
    }
}