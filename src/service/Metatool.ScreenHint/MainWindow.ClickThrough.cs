using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Metatool.ScreenHint;

public partial class MainWindow
{
	const int GWL_EXSTYLE = -20;
	const int WS_EX_TRANSPARENT = 0x00000020;

	[DllImport("user32.dll")]
	static extern int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("user32.dll")]
	static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		var hwnd = new WindowInteropHelper(this).Handle;
		var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
		SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
	}
}
