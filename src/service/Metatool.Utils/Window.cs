using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using Metatool.Service.MouseKey;
using Metatool.Utils.Implementation;
using Metatool.Utils.Internal;
using Condition = System.Windows.Automation.Condition;
using Point = System.Drawing.Point;

namespace Metatool.Service;

public class Window : IWindow
{
	public Window(IntPtr handle)
	{
		Handle = handle;
	}

	public IntPtr Handle { get; }
	public string Class => WindowHelper.GetClassName(Handle);

	/// <summary>
	/// cursor position of the caret in the active window, in screen coordinate. If there is no caret, return (0,0,0,0)
	/// </summary>
	public Rect CaretPosition
	{
		get
		{
			var guiInfo = new GUITHREADINFO();
			guiInfo.cbSize = (uint)Marshal.SizeOf(guiInfo);

			PInvokes.GetGUIThreadInfo(0, out guiInfo);

			var lt = new Point((int)guiInfo.rcCaret.Left, (int)guiInfo.rcCaret.Top);
			var rb = new Point((int)guiInfo.rcCaret.Right, (int)guiInfo.rcCaret.Bottom);

			PInvokes.ClientToScreen(guiInfo.hwndCaret, out lt);
			PInvokes.ClientToScreen(guiInfo.hwndCaret, out rb);
			//Console.WriteLine(lt.ToString() + rb.ToString());
			//SystemInformation.WorkingArea
			return new Rect(new System.Windows.Point() { X = lt.X, Y = lt.Y },
				new System.Windows.Point() { X = rb.X, Y = rb.Y });
		}
	}

	public Rect Rect
	{
		get
		{
			PInvokes.GetWindowRect(Handle, out var rect);
			return new Rect(new System.Windows.Point() { X = rect.Left, Y = rect.Top },
				new System.Windows.Point() { X = rect.Right, Y = rect.Bottom });
		}
	}

	public bool IsExplorerOrOpenSaveDialog
	{
		get
		{
			var c = Class;
			return "CabinetWClass" == c || "#32770" == c;
		}
	}

	public bool IsExplorer => "CabinetWClass" == Class;

	public bool IsOpenSaveDialog => "#32770" == Class;
	public bool IsTaskView => "XamlExplorerHostIslandWindow" == Class || "MultitaskingViewFrame" == Class;

	public AutomationElement UiAuto => AutomationElement.FromHandle(Handle);

	public void FocusControl(string className, string text)
	{
		var hWnd = PInvokes.GetForegroundWindow();
		var hControl = PInvokes.FindWindowEx(hWnd, IntPtr.Zero, className, text);
		PInvokes.SetFocus(hControl);
	}

	public AutomationElement FirstChild(Func<ConditionFactory, Condition> condition) => UiAuto.First(TreeScope.Children, condition);

	public AutomationElement FirstDescendant(Func<ConditionFactory, Condition> condition) => UiAuto.First(TreeScope.Descendants, condition);

	/// <summary>
	/// Sends key combination to this window via PostMessage.
	/// Keys are pressed in order, then released in reverse order.
	/// e.g. SendKey(KeyCodes.LWin, KeyCodes.Right) sends Win+Right to this window.
	/// </summary>
	public void SendKey(params KeyCodes[] keys)
	{
		const uint MAPVK_VK_TO_VSC = 0;

		foreach (var key in keys)
		{
			var scanCode = PInvokes.MapVirtualKey((uint)key, MAPVK_VK_TO_VSC);
			var lParam = (IntPtr)(0x00000001 | (scanCode << 16));
			PInvokes.PostMessage(Handle, (uint)WM.KEYDOWN, (IntPtr)key, lParam);
		}

		foreach (var key in keys.Reverse())
		{
			var scanCode = PInvokes.MapVirtualKey((uint)key, MAPVK_VK_TO_VSC);
			var lParam = (IntPtr)(0x00000001 | (scanCode << 16) | (0xC0 << 24));
			PInvokes.PostMessage(Handle, (uint)WM.KEYUP, (IntPtr)key, lParam);
		}
	}
}