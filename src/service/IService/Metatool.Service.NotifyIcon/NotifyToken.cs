using System;
using System.Windows.Controls.Primitives;

namespace Metatool.NotifyIcon;

public class NotifyToken
{
	internal Popup Popup;

	public NotifyToken(Popup popup = null)
	{
		Popup = popup;
	}

	public void Close()
	{
		Popup.IsOpen = false;
		Popup.Child  = null;

	}
}