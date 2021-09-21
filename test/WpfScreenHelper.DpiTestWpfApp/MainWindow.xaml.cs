using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfScreenHelper.DpiTestWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var screen in Screen.AllScreens)
            {
                Monitors.Items.Add(screen.DeviceName);
            }

            Monitors.SelectedIndex = 0;
        }

        private void ButtonCenter_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.Center,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonLeft_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.Left,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonRight_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.Right,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonTop_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.Top,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonBottom_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.Bottom,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonTopLeft_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.TopLeft,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonTopRight_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.TopRight,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonBottomRight_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.BottomRight,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }

        private void ButtonBottomLeft_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowPosition(this, WindowPositions.BottomLeft,
                Screen.AllScreens.ElementAt(Monitors.SelectedIndex).Bounds);
        }
        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private enum WindowPositions
        {
            Center,
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        }

        private static void SetWindowPosition(Window wnd, WindowPositions pos, Rect bounds)
        {
            var x = 0.0d;
            var y = 0.0d;

            switch (pos)
            {
                case WindowPositions.Center:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Left:
                    x = bounds.X;
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Top:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y;
                    break;
                case WindowPositions.Right:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Bottom:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
                case WindowPositions.TopLeft:
                    x = bounds.X;
                    y = bounds.Y;
                    break;
                case WindowPositions.TopRight:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y;
                    break;
                case WindowPositions.BottomRight:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
                case WindowPositions.BottomLeft:
                    x = bounds.X;
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
            }

            wnd.Left = x;
            wnd.Top = y;
        }
    }
}
