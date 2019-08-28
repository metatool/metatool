using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Metaseed.NotifyIcon;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Metaseed.UI.Notify;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Windows.Threading;
using Metaseed.NotifyIcon.Interop;
using Metaseed.UI.Implementation;
using Point = System.Windows.Point;

namespace Metaseed.MetaKeyboard
{
    public enum NotifyPosition
    {
        Default,
        ActiveScreen,
        ActiveWindowCenter,
        Caret
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

        public class MessageToken<T>
        {
            private readonly Popup           _popup;
            internal         DispatcherTimer _timer;
            public           bool            IsClosed;

            internal MessageToken(Popup popup)
            {
                _popup = popup;
            }

            public void Close()
            {
                if (IsClosed) return;
                var dataContext = _popup.DataContext as ObservableCollection<T>;
                dataContext.Clear();
                _popup.IsOpen = false;
                _timer?.Stop();
                IsClosed = true;
            }

            public void Refresh()
            {
                IsClosed      = false;
                _popup.IsOpen = true;
                _timer?.Stop();
                _timer?.Start();
            }
        }

        private static ObservableCollection<TipItem> selectActions;
        public static  MessageToken<TipItem>                  SelectionToken;

        public static MessageToken<TipItem> ShowSelectionAction(IEnumerable<(string des, Action action)> tips)
        {
            var valueTuples = tips.ToArray();
            var description =
                valueTuples.Select((d, i) =>
                {
                    i = selectActions?.Count ?? 0 + i;
                    string key;
                    if (i < 9)
                    {
                        key = i.ToString();
                    }
                    else
                    {
                        key = (Key.D0 + i).ToString();
                    }

                    return new TipItem()
                        {Key = key, DescriptionInfo = d.des, Action = d.action};
                });
            if (selectActions == null || (SelectionToken != null && SelectionToken.IsClosed))
            {
                selectActions = new ObservableCollection<TipItem>(description);
                var b = new SelectableMessage();
                return SelectionToken = ShowMessage(b, selectActions, 8888, NotifyPosition.Caret);
            }
            else
            {
                description.ToList().ForEach(tt => selectActions.Add(tt));
                SelectionToken.Refresh();
                return SelectionToken;
            }
        }

        public static MessageToken<TipItem> ShowMessage(System.Windows.FrameworkElement balloon,
            ObservableCollection<TipItem> data, int? timeout,
            NotifyPosition position = NotifyPosition.ActiveScreen, PopupAnimation animation = PopupAnimation.None)
        {
            var dispatcher = balloon.Dispatcher;
            if (!dispatcher.CheckAccess())
            {
                return dispatcher.Invoke(DispatcherPriority.Normal,
                        (Func<MessageToken<TipItem>>) (() => ShowMessage(balloon, data, timeout, position, animation))) as
                    MessageToken<TipItem>;
            }

            if (balloon == null) throw new ArgumentNullException("balloon");
            if (timeout.HasValue && timeout < 500)
            {
                var msg = "Invalid timeout of {0} milliseconds. Timeout must be at least 500 ms";
                msg = String.Format(msg, timeout);
                throw new ArgumentOutOfRangeException("timeout", msg);
            }

            if (LogicalTreeHelper.GetParent(balloon) is Popup parent)
            {
                parent.Child = null;
                var msg =
                    "Cannot display control [{0}] in a new balloon popup - that control already has a parent. You may consider creating new balloons every time you want to show one.";
                msg = String.Format(msg, balloon);
                throw new InvalidOperationException(msg);
            }

            var popup = new Popup();
            popup.AllowsTransparency = true;
            popup.PopupAnimation     = animation;
            popup.Placement          = PlacementMode.AbsolutePoint;
            popup.StaysOpen          = true;
            popup.DataContext      = data;

            Point point;
            switch (position)
            {
                case NotifyPosition.Caret:
                {
                    var rect = UI.Window.GetCurrentWindowCaretPosition();
                    var X    = (rect.Left   + rect.Width  / 2 - balloon.ActualWidth  / 2);
                    var Y    = (rect.Bottom + rect.Height / 2 - balloon.ActualHeight / 2);
                    if (X == 0 && Y == 0)
                    {
                        goto case NotifyPosition.ActiveWindowCenter;
                    }

                    point = new Point(X, Y);
                    break;
                }

                case NotifyPosition.ActiveWindowCenter:
                {
                    var rect = UI.Window.GetCurrentWindowRect();
                    var X    = (rect.X + rect.Width  / 2 - balloon.ActualWidth  / 2);
                    var Y    = (rect.Y + rect.Height / 2 - balloon.ActualHeight / 2);
                    point = new Point(X, Y);
                    break;
                }

                case NotifyPosition.ActiveScreen:
                {
                    var screen = Screen.FromHandle(UI.Window.CurrentWindowHandle);
                    if (screen.Equals(Screen.PrimaryScreen))
                    {
                        var p = TrayInfo.GetTrayLocation();
                        point = new Point(p.X, p.Y);
                        break;
                    }

                    var bounds = screen.Bounds;
                    var X      = bounds.X + bounds.Width;
                    var Y      = bounds.Y + bounds.Height;
                    point = new Point(X, Y);
                    break;
                }

                case NotifyPosition.Default:
                {
                    var p = TrayInfo.GetTrayLocation();
                    point = new Point(p.X, p.Y);
                    break;
                }


                default:
                    throw new ArgumentOutOfRangeException(nameof(position) + " not supported", position, null);
            }


            popup.Child            = balloon;
            popup.HorizontalOffset = point.X + 1;
            popup.VerticalOffset   = point.Y - 2;
            balloon.Focusable      = true;

            IInputElement element = null;
            popup.Opened += (s, a) =>
            {
                element = Keyboard.FocusedElement;
                var source = (HwndSource) PresentationSource.FromVisual(balloon);
                var handle = source.Handle;
                PInvokes.SetForegroundWindow(handle);
                Keyboard.Focus(balloon);
            };

            popup.IsOpen = true;
            popup.Focus();

            var r = new MessageToken<TipItem>(popup);
            popup.Closed += (s, a) =>
            {
                Keyboard.Focus(element);
                r.Close();
            };

            void TimerTick(object sender, EventArgs e)
            {
                r._timer.Tick -= TimerTick;
                r.Close();
            }

            if (timeout.HasValue)
            {
                r._timer      =  new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(timeout.Value)};
                r._timer.Tick += TimerTick;
                r._timer.Start();
            }

            return r;
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
                    throw new ArgumentOutOfRangeException(nameof(position) + " not supported", position, null);
            }

            trayIcon.CustomPopupPosition = func;
            return trayIcon.ShowCustomBalloon(control, PopupAnimation.None, timeout, onlyCloseByToken);
        }

        static Dictionary<string, IEnumerable<(string key, IEnumerable<string> descriptions)>> tipDictionary =
            new Dictionary<string, IEnumerable<(string key, IEnumerable<string> descriptions)>>();

        public static void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
            NotifyPosition position = NotifyPosition.ActiveScreen)
        {
            tipDictionary.TryGetValue(name, out var tp);
            if (tp != null && tips.SequenceEqual(tp)) return;

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
            NotifyPosition position = NotifyPosition.ActiveScreen)
        {
            if (tips == null) return;
            var description =
                tips.SelectMany(t => t.descriptions.Select(d => new TipItem() {Key = t.key, DescriptionInfo = d}));
            var t = new ObservableCollection<TipItem>(description);
            if (t.Count == 0) return;
            var keytipsBalloon = new FancyBalloon() {Tips = t};
            ShowMessage(keytipsBalloon, 88888);
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
                tips.SelectMany(t => t.descriptions.Select(d => new TipItem() {Key = t.key, DescriptionInfo = d}));
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