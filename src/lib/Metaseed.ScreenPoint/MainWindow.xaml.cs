using System;
using System.Windows;
using System.Windows.Threading;

namespace Metaseed.ScreenHint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static MainWindow Inst = new MainWindow();
        private DispatcherTimer timer;
        private MainWindow()
        {
            InitializeComponent();
            Hide();
            Inst = this;
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(800), DispatcherPriority.Input,
                (o, e) =>
                {
                    highLight.Visibility = Visibility.Hidden;
                    Hide();
                    timer.Stop();
                }, Dispatcher.CurrentDispatcher);
            timer.Stop();
        }

        public void HighLight(Rect rect)
        {
            System.Windows.Controls.Canvas.SetLeft(highLight, rect.Left);
            System.Windows.Controls.Canvas.SetTop(highLight, rect.Top);
            highLight.Width = rect.Width;
            highLight.Height = rect.Height;
            highLight.Visibility = Visibility.Visible;
            timer.Start();
        }
    }
}
