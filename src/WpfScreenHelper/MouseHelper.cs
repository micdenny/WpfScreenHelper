using System.Windows;

namespace WpfScreenHelper
{
    public static class MouseHelper
    {
        public static Point MousePosition
        {
            get
            {
                var pt = new NativeMethods.POINT();
                NativeMethods.GetCursorPos(pt);
                var point = new Point(pt.x, pt.y);

                if (Screen.PerMonitorDpiAware)
                {
                    var pointStruct = new NativeMethods.POINTSTRUCT((int) point.X, (int) point.Y);

                    var screen = Screen.FromPoint(pointStruct);

                    if (!screen.ScaleFactor.Equals(1.0))
                        point = new Point(pt.x / screen.ScaleFactor, pt.y / screen.ScaleFactor);
                }

                return point;
            }
        }
    }
}