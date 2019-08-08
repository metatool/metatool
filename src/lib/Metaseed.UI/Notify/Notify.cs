using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Metaseed.NotifyIcon;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Media;
using Metaseed.UI.Notify;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Windows.Threading;
using Metaseed.NotifyIcon.Interop;

namespace Metaseed.MetaKeyboard
{
    public enum NotifyPosition
    {
        Default,
        ActiveScreen,
        ActiveWindowCenter
    }

    public class Notify
    {
        private static TaskbarIcon trayIcon;

        static Notify()
        {
            var foo = new Uri("pack://application:,,,/Metaseed.UI;component/Notify/NotifyIconResources.xaml",
                UriKind.RelativeOrAbsolute);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() {Source = foo});
            trayIcon = Application.Current.FindResource("NotifyIcon") as TaskbarIcon;
        }

        public static MenuItem AddContextMenuItem(string header, Action<MenuItem> excute,
            Func<MenuItem, bool> canExcute = null, bool isCheckable = false, bool? initialState = null)
        {
            var item                                  = new MenuItem() {Header = header, IsCheckable = isCheckable};
            if (initialState.HasValue) item.IsChecked = initialState.Value;
            item.Command = new DelegateCommand<MenuItem>()
                {CanExecuteFunc = canExcute, CommandAction = excute};
            item.CommandParameter = item;
            trayIcon.ContextMenu.Items.Insert(0, item);

            return item;
        }

        public static void ShowMessage(string msg)
        {
            if (msg == "") return;
            trayIcon.ShowBalloonTip(string.Empty, msg, BalloonIcon.None);
        }

        public static CloseToken ShowMessage(System.Windows.FrameworkElement control, int? timeout,
            NotifyPosition position = NotifyPosition.ActiveScreen, bool onlyCloseByToken = false)
        {
            TaskbarIcon.GetCustomPopupPosition func = null;
            switch (position)
            {
                case NotifyPosition.ActiveWindowCenter:
                    func = () =>
                    {
                        var rect = UI.Window.GetCurrentWindowRect();
                        return new NotifyIcon.Interop.Point()
                        {
                            X = (int) (rect.X + rect.Width  / 2 - control.ActualWidth  / 2),
                            Y = (int) (rect.Y + rect.Height / 2 - control.ActualHeight / 2)
                        };
                    };
                    break;
                case NotifyPosition.ActiveScreen:
                    func = () =>
                    {
                        var screen = Screen.FromHandle(UI.Window.CurrentWindowHandle);
                        if (screen.Equals(Screen.PrimaryScreen))
                        {
                            return trayIcon.GetPopupTrayPosition();
                        }

                        var bounds = screen.Bounds;
                        return new NotifyIcon.Interop.Point()
                        {
                            X = bounds.X + bounds.Width,
                            Y = bounds.Y + bounds.Height
                        };
                    };
                    break;
                case NotifyPosition.Default:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            trayIcon.CustomPopupPosition = func;
            return trayIcon.ShowCustomBalloon(control, PopupAnimation.None, timeout, onlyCloseByToken);
        }

        static Dictionary<string, IEnumerable<(string key, IEnumerable<string> descriptions)>> tipDictionary =
            new Dictionary<string, IEnumerable<(string key, IEnumerable<string> descriptions)>>();

        public static void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
            NotifyPosition position =
                NotifyPosition.ActiveScreen)
        {
            tipDictionary.TryGetValue(name, out var tp);
            if (tp!=null && tips.SequenceEqual(tp)) return;

            tipDictionary[name] = tips;
            var t = tipDictionary.SelectMany(pair => pair.Value).ToArray();
            if (t.Any())
                ShowKeysTip(t, position);
            else
            {
                CloseKeysTip();
            }
        }

        public static void ShowKeysTip(IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
            NotifyPosition position =
                NotifyPosition.ActiveScreen)
        {
            if (tips == null) return;
            var description =
                tips.SelectMany(t => t.descriptions.Select(d => new TipItem() {key = t.key, Description = d}));
            var t = new ObservableCollection<TipItem>(description);
            if (t.Count == 0) return;
            var b = new FancyBalloon() {Tips = t};
            ShowMessage(b, 88888);
        }

        public static void CloseKeysTip(string name)
        {
            ShowKeysTip(name, Enumerable.Empty<(string key, IEnumerable<string> descriptions)>());
        }

        public static void CloseKeysTip()
        {
            trayIcon.CloseBalloon();
        }

        public static void ShowKeysTip1(IEnumerable<(string key, IEnumerable<string> descriptions)> tips)
        {
            if (tips == null) return;
            var description =
                tips.SelectMany(t => t.descriptions.Select(d => new TipItem() {key = t.key, Description = d}));
            var b = new FancyBalloon() {Tips = new ObservableCollection<TipItem>(description)};


            var popup = new Popup()
            {
                Placement = PlacementMode.AbsolutePoint, StaysOpen = false, Child = b
            };

            var bounds = Screen.FromHandle(UI.Window.CurrentWindowHandle).Bounds;

            var window = new Window()
            {
                ResizeMode    = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Width         = 0,
                Height        = 0,
                Background    = Brushes.Transparent,
                Content       = popup
            };
            window.Loaded += (o, e) =>
            {
                popup.HorizontalOffset = bounds.X + bounds.Width - 1;
                popup.VerticalOffset   = bounds.Y                + bounds.Height;
                popup.IsOpen           = true;
                var timer = new DispatcherTimer() {Interval = TimeSpan.FromSeconds(20)};
                timer.Tick += (o1, e1) =>
                {
                    popup.IsOpen = false;
                    timer.Stop();
                    window.Close();
                };
                timer.Start();
            };
            window.Show();
        }

        private static void CenterWindowOnScreen(Window window)
        {
            double screenWidth  = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth  = window.Width;
            double windowHeight = window.Height;
            window.Left = (screenWidth  / 2) - (windowWidth  / 2);
            window.Top  = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}