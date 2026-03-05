using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using Metatool.Utils.Internal;
using Metatool.WindowsInput;

namespace Metatool.Service;

public class WindowManager : IWindowManager
{
	private ActiveWindowMonitor _activeWindowMonitor = new();

	public event ActiveWindowChangedHandler ActiveWindowChanged
	{
		add { _activeWindowMonitor.ActiveWindowChanged += value; }
		remove { _activeWindowMonitor.ActiveWindowChanged -= value; }
	}

	public IWindow CurrentWindow => new Window(PInvokes.GetForegroundWindow());

	public IWindow WindowWithMouse
	{
		get
		{
			var hwnd = ControlWithMouse;
			if (hwnd == IntPtr.Zero)
				return null;
			hwnd = PInvokes.GetAncestor(hwnd, PInvokes.GA_ROOT);
			return new Window(hwnd);
		}
	}


	public IntPtr ControlWithMouse
	{
		get
		{
			PInvokes.GetCursorPos(out Point point);
			return PInvokes.WindowFromPoint(point);
		}
	}

	public IWindow Show(IntPtr hWnd)
	{
		PInvokes.ShowWindowAsync(hWnd, PInvokes.SW.Show);
		PInvokes.SetForegroundWindow(hWnd);
		return new Window(hWnd);
	}

	[DllImport("user32.dll")]
	static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	public string GetWindowTitle(IntPtr hwnd)
	{
		StringBuilder Buff = new StringBuilder(500);
		GetWindowText(hwnd, Buff, Buff.Capacity);
		return Buff.ToString();
	}

}