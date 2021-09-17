namespace WpfScreenHelper
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    using WpfScreenHelper.Enum;

    /// <summary>
    ///     Provides helper functions for window class.
    /// </summary>
    public static class WindowHelper
    {
        public static void SetWindowPosition(this Window window, WindowPositions pos, Rect bounds)
        {
            var coordinates = CalculateWindowCoordinates(window, pos, bounds);

            // correct resulting coordinates to match bounds
            if (bounds.X > coordinates.X)
            {
                coordinates.X = bounds.X;
            }
            if (bounds.Y > coordinates.Y)
            {
                coordinates.Y = bounds.Y;
            }
            if (bounds.Width < coordinates.Width)
            {
                coordinates.Width = bounds.Width;
            }
            if (bounds.Height < coordinates.Height)
            {
                coordinates.Height = bounds.Height;
            }

            // The first move puts it on the correct monitor, which triggers WM_DPICHANGED
            // The +1/-1 coerces WPF to update Window.Top/Left/Width/Height in the second move
            window.Left = coordinates.X + 1;
            window.Top = coordinates.Y;
            window.Width = coordinates.Width + 1;
            window.Height = coordinates.Height;

            window.Left = coordinates.X;
            window.Top = coordinates.Y;
            window.Width = coordinates.Width;
            window.Height = coordinates.Height;
        }

        public static void MaximizeWindowToVirtualScreen(this Window window)
        {
            var virtualDisplay = SystemInformation.WpfVirtualScreen;

            window.Left = virtualDisplay.Left;
            window.Top = virtualDisplay.Top;
            window.Width = virtualDisplay.Width;
            window.Height = virtualDisplay.Height;
        }

        public static Rect GetWindowAbsolutePlacement(this Window window)
        {
            var screen = Screen.FromWindow(window);

            var left = Math.Abs(screen.Bounds.Left - window.Left) * screen.ScaleFactor;
            var top = Math.Abs(screen.Bounds.Top - window.Top) * screen.ScaleFactor;
            var width = window.Width * screen.ScaleFactor;
            var height = window.Height * screen.ScaleFactor;

            return new Rect(left, top, width, height);
        }

        private static Rect CalculateWindowCoordinates(FrameworkElement window, WindowPositions pos, Rect bounds)
        {
            switch (pos)
            {
                case WindowPositions.Center:
                    {
                        var x = bounds.X + ((bounds.Width - window.Width) / 2.0);
                        var y = bounds.Y + ((bounds.Height - window.Height) / 2.0);

                        return new Rect(x, y, window.Width, window.Height);
                    }

                case WindowPositions.Left:
                    {
                        var y = bounds.Y + ((bounds.Height - window.Height) / 2.0);

                        return new Rect(bounds.X, y, window.Width, window.Height);
                    }

                case WindowPositions.Top:
                    {
                        var x = bounds.X + ((bounds.Width - window.Width) / 2.0);

                        return new Rect(x, bounds.Y, window.Width, window.Height);
                    }

                case WindowPositions.Right:
                    {
                        var x = bounds.X + (bounds.Width - window.Width);
                        var y = bounds.Y + ((bounds.Height - window.Height) / 2.0);

                        return new Rect(x, y, window.Width, window.Height);
                    }

                case WindowPositions.Bottom:
                    {
                        var x = bounds.X + ((bounds.Width - window.Width) / 2.0);
                        var y = bounds.Y + (bounds.Height - window.Height);

                        return new Rect(x, y, window.Width, window.Height);
                    }

                case WindowPositions.TopLeft:
                    return new Rect(bounds.X, bounds.Y, window.Width, window.Height);

                case WindowPositions.TopRight:
                    {
                        var x = bounds.X + (bounds.Width - window.Width);

                        return new Rect(x, bounds.Y, window.Width, window.Height);
                    }

                case WindowPositions.BottomRight:
                    {
                        var x = bounds.X + (bounds.Width - window.Width);
                        var y = bounds.Y + (bounds.Height - window.Height);

                        return new Rect(x, y, window.Width, window.Height);
                    }

                case WindowPositions.BottomLeft:
                    {
                        var y = bounds.Y + (bounds.Height - window.Height);

                        return new Rect(bounds.X, y, window.Width, window.Height);
                    }

                case WindowPositions.Maximize:
                    return bounds;

                default:
                    return Rect.Empty;
            }
        }
    }
}
