using System;
using System.Windows;
using System.Windows.Automation;
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
	AutomationElement UiAuto { get; }
	void FocusControl(string className, string text);
	AutomationElement FirstChild(Func<ConditionFactory, Condition> condition);
	AutomationElement FirstDescendant(Func<ConditionFactory, Condition> condition);
}