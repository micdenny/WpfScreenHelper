using System;
using System.Windows;
using System.Windows.Interop;
using WpfScreenHelper.Enum;

namespace WpfScreenHelper
{
    /// <summary>
    /// Provides helper functions for window class.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Moves window to desired position on screen.
        /// </summary>
        /// <param name="window">Window to move.</param>
        /// <param name="x">X coordinate for moving.</param>
        /// <param name="y">Y coordinate for moving.</param>
        /// <param name="width">New width of the window.</param>
        /// <param name="height">New height of the window.</param>
        public static void SetWindowPosition(this Window window, int x, int y, int width, int height)
        {
            // The first move puts it on the correct monitor, which triggers WM_DPICHANGED
            // The +1/-1 coerces WPF to update Window.Top/Left/Width/Height in the second move
            NativeMethods.MoveWindow(new WindowInteropHelper(window).Handle, x - 1, y, width + 1, height, false);
            NativeMethods.MoveWindow(new WindowInteropHelper(window).Handle, x, y, width, height, true);
        }

        /// <summary>
        /// Moves window to desired position on screen.
        /// </summary>
        /// <param name="window">Window to move.</param>
        /// <param name="pos">Desired position.</param>
        /// <param name="screen">The screen to which we move.</param>
        public static void SetWindowPosition(this Window window, WindowPositions pos, Screen screen)
        {
            var coordinates = CalculateWindowCoordinates(window, pos, screen);

            window.SetWindowPosition((int)coordinates.X, (int)coordinates.Y, (int)coordinates.Width, (int)coordinates.Height);
        }

        /// <summary>
        /// Gets window position on screen with respect of screen scale factor.
        /// </summary>
        public static Rect GetWindowAbsolutePlacement(this Window window)
        {
            var placement = GetWindowPlacement(window);

            return new Rect(
                Math.Abs(placement.Left),
                Math.Abs(placement.Top),
                placement.Width,
                placement.Height);
        }

        public static Rect GetWindowPlacement(this Window window)
        {
            var screen = Screen.FromWindow(window);

            var left = (screen.WpfBounds.Left - window.Left) * screen.ScaleFactor;
            var top = (screen.WpfBounds.Top - window.Top) * screen.ScaleFactor;
            var width = window.Width * screen.ScaleFactor;
            var height = window.Height * screen.ScaleFactor;

            return new Rect(left, top, width, height);
        }

        /// <summary>
        /// Calculates window end position.
        /// </summary>
        private static Rect CalculateWindowCoordinates(FrameworkElement window, WindowPositions pos, Screen screen)
        {
            switch (pos)
            {
                case WindowPositions.Center:
                    {
                        var x = screen.WpfBounds.X + (screen.WpfBounds.Width - window.Width) / 2.0;
                        var y = screen.WpfBounds.Y + (screen.WpfBounds.Height - window.Height) / 2.0;

                        return new Rect(x * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.Left:
                    {
                        var y = screen.WpfBounds.Y + (screen.WpfBounds.Height - window.Height) / 2.0;

                        return new Rect(screen.WpfBounds.X * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.Top:
                    {
                        var x = screen.WpfBounds.X + (screen.WpfBounds.Width - window.Width) / 2.0;

                        return new Rect(x * screen.ScaleFactor, screen.WpfBounds.Y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.Right:
                    {
                        var x = screen.WpfBounds.X + (screen.WpfBounds.Width - window.Width);
                        var y = screen.WpfBounds.Y + (screen.WpfBounds.Height - window.Height) / 2.0;

                        return new Rect(x * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.Bottom:
                    {
                        var x = screen.WpfBounds.X + (screen.WpfBounds.Width - window.Width) / 2.0;
                        var y = screen.WpfBounds.Y + (screen.WpfBounds.Height - window.Height);

                        return new Rect(x * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.TopLeft:
                    return new Rect(screen.WpfBounds.X * screen.ScaleFactor, screen.WpfBounds.Y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);

                case WindowPositions.TopRight:
                    {
                        var x = screen.WpfBounds.X + (screen.WpfBounds.Width - window.Width);

                        return new Rect(x * screen.ScaleFactor, screen.WpfBounds.Y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.BottomRight:
                    {
                        var x = screen.WpfBounds.X + (screen.WpfBounds.Width - window.Width);
                        var y = screen.WpfBounds.Y + (screen.WpfBounds.Height - window.Height);

                        return new Rect(x * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.BottomLeft:
                    {
                        var y = screen.WpfBounds.Y + (screen.WpfBounds.Height - window.Height);

                        return new Rect(screen.WpfBounds.X * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
                    }

                case WindowPositions.Maximize:
                    return new Rect(screen.WpfBounds.X * screen.ScaleFactor, screen.WpfBounds.Y * screen.ScaleFactor, screen.WpfBounds.Width * screen.ScaleFactor, screen.WpfBounds.Height * screen.ScaleFactor);

                default:
                    return Rect.Empty;
            }
        }
    }
}