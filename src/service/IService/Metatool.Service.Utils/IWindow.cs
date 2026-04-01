using System;
using System.Windows;
using System.Windows.Automation;
using Metatool.Service.MouseKey;
using Condition = System.Windows.Automation.Condition;

namespace Metatool.Service;

public interface IWindow
{
	IntPtr Handle { get; }
	string Class { get; }
	Rect CaretPosition { get; }
	Rect Rect { get; }
	bool IsExplorerOrOpenSaveDialog { get; }
	bool IsExplorer { get; }
	bool IsOpenSaveDialog { get; }
	bool IsTaskView { get; }
	AutomationElement UiAuto { get; }
	void FocusControl(string className, string text);
	AutomationElement FirstChild(Func<ConditionFactory, Condition> condition);
	AutomationElement FirstDescendant(Func<ConditionFactory, Condition> condition);
	bool Contains(IntPtr hCtrl);
	void SendKey(params KeyCodes[] keys);

	/// <summary>
	/// Briefly highlights the window border to provide visual feedback.
	/// Uses DWM border color on Windows 11+, falls back to FlashWindowEx on older versions.
	/// </summary>
	/// <param name="durationMs">How long the highlight lasts in milliseconds (default 500)</param>
	void Highlight(int durationMs = 500);
}