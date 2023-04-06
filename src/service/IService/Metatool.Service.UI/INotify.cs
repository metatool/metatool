using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Metatool.NotifyIcon;
using Metatool.UI;

namespace Metatool.Service;

public interface INotify
{
	void ShowMessage(string msg);
	NotifyToken ShowMessage(System.Windows.FrameworkElement control, int? timeout,
		NotifyPosition position = NotifyPosition.ActiveScreen, bool onlyCloseByToken = false);

	MenuItem AddContextMenuItem(string header, Action<MenuItem> execute,
		Func<MenuItem, bool> canExecute = null, bool isCheckable = false, bool? isChecked = null);

	void ShowKeysTip(IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
		NotifyPosition position = NotifyPosition.ActiveScreen);

	void ShowKeysTip(string name, IEnumerable<(string key, IEnumerable<string> descriptions)> tips,
		NotifyPosition position = NotifyPosition.ActiveScreen);

	void CloseKeysTip(string name);

	void CloseKeysTip();

	MessageToken<TipItem> ShowSelectionAction(IEnumerable<(string des, Action action)> tips, Action<int> closeViaKey=null);
}