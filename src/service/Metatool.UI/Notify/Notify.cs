using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Metatool.NotifyIcon;
using Metatool.NotifyIcon.Interop;
using Metatool.Service;
using Metatool.Utils.Internal;
using UI.Notify;
using Application = System.Windows.Application;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace Metatool.UI.Notify;

public partial class Notify: INotify
{
	private static  TaskbarIcon _trayIcon;
	private static IWindowManager _windowManager;
	private static IWindowManager WindowManager => _windowManager ??= Services.Get<IWindowManager>();
	private static TaskbarIcon TrayIcon
	{
		get
		{
			if (Application.Current == null) return null;
			if (_trayIcon != null) return _trayIcon;
			var resource = new Uri("pack://application:,,,/Metatool.UI;component/Notify/NotifyIconResources.xaml",
				UriKind.RelativeOrAbsolute);
			Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = resource });
			_trayIcon = Application.Current.FindResource("NotifyIcon") as TaskbarIcon;
			return _trayIcon;
		}
	}

	public static MessageToken<TipItem> ShowMessage(FrameworkElement balloon,
		ObservableCollection<TipItem> data, int? timeout,
		NotifyPosition position = NotifyPosition.ActiveScreen, PopupAnimation animation = PopupAnimation.None)
	{
		var dispatcher = balloon.Dispatcher;
		if (!dispatcher.CheckAccess())
		{
			return dispatcher.Invoke(DispatcherPriority.Normal,
					(Func<MessageToken<TipItem>>) (() =>
						ShowMessage(balloon, data, timeout, position, animation))) as
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
		popup.DataContext        = data;

		Point point;
		switch (position)
		{
			case NotifyPosition.Caret:
			{
				var rect = WindowManager.CurrentWindow.CaretPosition;
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
				var rect = WindowManager.CurrentWindow.Rect;
				var X    = (rect.X + rect.Width  / 2 - balloon.ActualWidth  / 2);
				var Y    = (rect.Y + rect.Height / 2 - balloon.ActualHeight / 2);
				point = new Point(X, Y);
				break;
			}

			case NotifyPosition.ActiveScreen:
			{
				var screen = Screen.FromHandle(WindowManager.CurrentWindow.Handle);
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
			r.Timer.Tick -= TimerTick;
			r.Close();
		}

		if (timeout.HasValue)
		{
			r.Timer      =  new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(timeout.Value)};
			r.Timer.Tick += TimerTick;
			r.Timer.Start();
		}

		return r;
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

		var bounds = Screen.FromHandle(WindowManager.CurrentWindow.Handle).Bounds;

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
		double screenWidth  = SystemParameters.PrimaryScreenWidth;
		double screenHeight = SystemParameters.PrimaryScreenHeight;
		double windowWidth  = window.Width;
		double windowHeight = window.Height;
		window.Left = (screenWidth  / 2) - (windowWidth  / 2);
		window.Top  = (screenHeight / 2) - (windowHeight / 2);
	}
}