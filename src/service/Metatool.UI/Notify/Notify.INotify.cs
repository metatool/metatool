﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Metatool.NotifyIcon;
using Metatool.Utils.Notify;
using UI.Notify;
using MenuItem = System.Windows.Controls.MenuItem;
using Application = System.Windows.Application;

namespace Metatool.UI.Notify;

public partial class Notify
{
	public void ShowMessage(string msg)
	{
		if (msg == "") return;
		TrayIcon?.ShowBalloonTip(string.Empty, msg, BalloonIcon.None);
	}

	public NotifyToken ShowMessage(System.Windows.FrameworkElement control, int? timeout,
		NotifyPosition position = NotifyPosition.ActiveScreen, bool onlyCloseByToken = false)
	{
		TaskbarIcon.GetCustomPopupPosition func = null;
		switch (position)
		{
			case NotifyPosition.ActiveWindowCenter:
				func = () =>
				{
					var rect = WindowManager.CurrentWindow.Rect;
					return new NotifyIcon.Interop.Point()
					{
						X = (int) (rect.X + rect.Width / 2 - control.ActualWidth / 2),
						Y = (int) (rect.Y + rect.Height / 2 - control.ActualHeight / 2)
					};
				};
				break;
			case NotifyPosition.ActiveScreen:
				func = () =>
				{
					var screen = Screen.FromHandle(WindowManager.CurrentWindow.Handle);
					if (screen.Equals(Screen.PrimaryScreen))
					{
						return TrayIcon.GetPopupTrayPosition();
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

		TrayIcon.CustomPopupPosition = func;
		return TrayIcon.ShowCustomBalloon(control, PopupAnimation.None, timeout, onlyCloseByToken);
	}

	public MenuItem AddContextMenuItem(string header, Action<MenuItem> execute,
		Func<MenuItem, bool> canExecute = null, bool isCheckable = false, bool? isChecked = null)
	{
		var item = new MenuItem() {Header = header, IsCheckable = isCheckable};
		if (isChecked.HasValue)
			item.IsChecked = isChecked.Value;
		item.Command = new DelegateCommand<MenuItem>()
			{CanExecuteFunc = canExecute, CommandAction = execute};
		item.CommandParameter = item;
		TrayIcon.ContextMenu?.Items.Insert(0, item);

		return item;
	}

	public void ShowKeysTip(IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
		NotifyPosition position = NotifyPosition.ActiveScreen)
	{
		if (tips == null) return;
            
		var description =
			tips.SelectMany(t => t.descriptions.Select(d => new TipItem() {Key = t.key, DescriptionInfo = d}));
		var t = new ObservableCollection<TipItem>(description);
		if (t.Count == 0) return;

		var keytipsBalloon = new FancyBalloon() {Tips = t};
		ShowMessage(keytipsBalloon, 8888);
	}

	readonly Dictionary<string, IEnumerable<(string key, IEnumerable<string> descriptions)>> _tipDictionary = new();

	public void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
		NotifyPosition position = NotifyPosition.ActiveScreen)
	{
		var keyAndTips = tips as (string key, IEnumerable<string> descriptions)[] ?? tips.ToArray();
		if (_tipDictionary.Count == 0 && !keyAndTips.Any())
		{
			return;
		}

		_tipDictionary.TryGetValue(name, out var tp);
		if (tp != null && keyAndTips.SequenceEqual(tp)) return;

		_tipDictionary[name] = keyAndTips;
		var t = _tipDictionary.SelectMany(pair => pair.Value).ToList();
		t.Sort((a,b)=>string.Compare(a.key, b.key, StringComparison.Ordinal));
		if (t.Any())
			ShowKeysTip(t, position);
		else
		{
			CloseKeysTip();
		}
	}


	public void CloseKeysTip(string name)
	{
		ShowKeysTip(name, Enumerable.Empty<(string key, IEnumerable<string> descriptions)>());
	}

	public void CloseKeysTip()
	{
		if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
			Application.Current.Dispatcher.BeginInvoke((Action)CloseKeysTip);

		TrayIcon?.CloseBalloon();
	}

	private ObservableCollection<TipItem> selectActions;
	public MessageToken<TipItem> SelectionToken;

	public MessageToken<TipItem> ShowSelectionAction(IEnumerable<(string des, Action action)> tips,
		Action<int> closeViaKey = null)
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
					key = (Keys.D0 + i).ToString();
				}

				return new TipItem()
					{Key = key, DescriptionInfo = d.des, Action = d.action};
			});
		if (selectActions == null || (SelectionToken != null && SelectionToken.IsClosed))
		{
			selectActions = new ObservableCollection<TipItem>(description);
			var b = new SelectableMessage();
			b.CloseViaKey += closeViaKey;
			return SelectionToken = ShowMessage(b, selectActions, 8888, NotifyPosition.Caret);
		}
		else
		{
			description.ToList().ForEach(tt => selectActions.Add(tt));
			SelectionToken.Refresh();
			return SelectionToken;
		}
	}
}