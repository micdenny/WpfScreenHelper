using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfScreenHelper.Enum;

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

        private void ButtonFillToScreen_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.Fill, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonCenter_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.Center, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonLeft_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.Left, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonRight_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.Right, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonTop_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.Top, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonBottom_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.Bottom, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonTopLeft_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.TopLeft, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonTopRight_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.TopRight, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonBottomRight_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.BottomRight, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void ButtonBottomLeft_OnClick(object sender, RoutedEventArgs e)
        {
            this.SetWindowPosition(WindowPositions.BottomLeft, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonOrigine_OnClick(object sender, RoutedEventArgs e)
        {
            this.Height = 400;
            this.Width = 700;
            this.SetWindowPosition(WindowPositions.Center, Screen.AllScreens.ElementAt(Monitors.SelectedIndex));

        }
    }
}