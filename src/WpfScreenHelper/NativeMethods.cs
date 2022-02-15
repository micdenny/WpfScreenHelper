using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;

namespace WpfScreenHelper
{
    internal static class NativeMethods
    {
        public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

        public enum DpiType
        {
            EFFECTIVE = 0,
            ANGULAR = 1,
            RAW = 2
        }

        public enum SystemMetric
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
            SM_XVIRTUALSCREEN = 76,
            SM_YVIRTUALSCREEN = 77,
            SM_CXVIRTUALSCREEN = 78,
            SM_CYVIRTUALSCREEN = 79,
            SM_CMONITORS = 80
        }

        public enum SPI : uint
        {
            /// <summary>
            /// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen not obscured
            /// by the system taskbar or by application desktop toolbars. The pvParam parameter must point to a RECT structure that receives
            /// the coordinates of the work area, expressed in virtual screen coordinates.
            /// To get the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
            /// </summary>
            SPI_GETWORKAREA = 0x0030
        }

        [Flags]
        public enum SPIF
        {
            None = 0x00,

            /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
            SPIF_UPDATEINIFILE = 0x01,

            /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
            SPIF_SENDCHANGE = 0x02,

            /// <summary>Same as SPIF_SENDCHANGE.</summary>
            SPIF_SENDWININICHANGE = 0x02
        }

        public enum MonitorDefault
        {
            /// <summary>If the point is not contained within any display monitor, return a handle to the display monitor that is nearest to the point.</summary>
            MONITOR_DEFAULTTONEAREST = 0x00000002,

            /// <summary>If the point is not contained within any display monitor, return NULL.</summary>
            MONITOR_DEFAULTTONULL = 0x00000000,

            /// <summary>If the point is not contained within any display monitor, return a handle to the primary display monitor.</summary>
            MONITOR_DEFAULTTOPRIMARY = 0x00000001
        }

        public enum D2D1_FACTORY_TYPE
        {
            D2D1_FACTORY_TYPE_SINGLE_THREADED = 0,
            D2D1_FACTORY_TYPE_MULTI_THREADED = 1,
        }

        public static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [DllImport(ExternDll.Shcore, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In][Out] MONITORINFOEX info);

        [DllImport(ExternDll.User32, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool EnumDisplayMonitors(HandleRef hdc, COMRECT rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport(ExternDll.User32, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetSystemMetrics(SystemMetric nIndex);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SystemParametersInfo(SPI nAction, int nParam, ref RECT rc, SPIF nUpdate);

        [DllImport(ExternDll.User32, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, MonitorDefault flags);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetCursorPos([In][Out] POINT pt);

        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern bool IsProcessDPIAware();

        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport(ExternDll.D2D1)]
        public static extern int D2D1CreateFactory(D2D1_FACTORY_TYPE factoryType, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, IntPtr pFactoryOptions, out ID2D1Factory ppIFactory);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rect r)
            {
                left = (int)r.Left;
                top = (int)r.Top;
                right = (int)r.Right;
                bottom = (int)r.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }

            public Size Size => new Size(right - left, bottom - top);
        }

        // use this in cases where the Native API takes a POINT not a POINT*
        // classes marshal by ref.
        [StructLayout(LayoutKind.Sequential)]
        public struct POINTSTRUCT
        {
            public int x;
            public int y;

            public POINTSTRUCT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;

            public POINT()
            {
            }

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

#if DEBUG

            public override string ToString()
            {
                return "{x=" + x + ", y=" + y + "}";
            }

#endif
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MONITORINFOEX
        {
            internal int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));

            internal RECT rcMonitor = new RECT();
            internal RECT rcWork = new RECT();
            internal int dwFlags = 0;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal char[] szDevice = new char[32];
        }

        [StructLayout(LayoutKind.Sequential)]
        public class COMRECT
        {
            public int bottom;
            public int left;
            public int right;
            public int top;

            public COMRECT()
            {
            }

            public COMRECT(Rect r)
            {
                left = (int)r.X;
                top = (int)r.Y;
                right = (int)r.Right;
                bottom = (int)r.Bottom;
            }

            public COMRECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public static COMRECT FromXYWH(int x, int y, int width, int height)
            {
                return new COMRECT(x, y, x + width, y + height);
            }

            public override string ToString()
            {
                return "Left = " + left + " Top " + top + " Right = " + right + " Bottom = " + bottom;
            }
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("06152247-6f50-465a-9245-118bfd3b6007")]
        internal interface ID2D1Factory
        {
            int ReloadSystemMetrics();

            [PreserveSig]
            void GetDesktopDpi(out float dpiX, out float dpiY);

            // the rest is not implemented as we don't need it
        }
    }
}