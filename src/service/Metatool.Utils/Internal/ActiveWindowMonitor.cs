using Metatool.Service;
using System;
using System.Runtime.InteropServices;

namespace Metatool.WindowsInput;

internal class ActiveWindowMonitor
{
	public event ActiveWindowChangedHandler ActiveWindowChanged;

	[DllImport("user32.dll")]
	static extern IntPtr GetForegroundWindow();

	delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
		IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
		uint dwmsEventTime);

	const uint WINEVENT_OUTOFCONTEXT = 0;
	const uint EVENT_SYSTEM_FOREGROUND = 3;

	[DllImport("user32.dll")]
	static extern bool UnhookWinEvent(IntPtr hWinEventHook);

	[DllImport("user32.dll")]
	static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
		IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
		uint idProcess, uint idThread, uint dwFlags);

	IntPtr m_hhook;
	private WinEventDelegate _winEventProc;

	public ActiveWindowMonitor()
	{
		_winEventProc = new WinEventDelegate(WinEventProc);
		m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
			EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc,
			0, 0, WINEVENT_OUTOFCONTEXT);
	}

	void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
		int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		if (eventType == EVENT_SYSTEM_FOREGROUND)
		{
			if (ActiveWindowChanged != null)
				ActiveWindowChanged(this, hwnd);
		}
	}

	~ActiveWindowMonitor()
	{
		UnhookWinEvent(m_hhook);
	}
}