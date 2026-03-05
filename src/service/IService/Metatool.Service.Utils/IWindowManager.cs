using System;

namespace Metatool.Service;

public delegate void ActiveWindowChangedHandler(object sender, IntPtr hwnd);

public interface IWindowManager
{
	event ActiveWindowChangedHandler ActiveWindowChanged;
	IWindow CurrentWindow { get; }
	/// <summary>
	/// get window from mouse cursor
	/// </summary>
	IWindow WindowWithMouse { get; }
	IntPtr ControlWithMouse { get; }
	IWindow Show(IntPtr hWnd);
}