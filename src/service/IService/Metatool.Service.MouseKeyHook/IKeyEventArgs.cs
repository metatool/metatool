﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Metatool.Service;

public interface IKeyPressEventArgs
{
	char KeyChar { get; set; }
	bool Handled { get; set; }
	bool IsNonChar { get; }
	int Timestamp { get; }
	IKeyEventArgs EventArgs { get; }
	bool IsActive(ISequenceUnit hotKey) => EventArgs.IsActive(hotKey);
}
public interface IKeyEventArgs
{
	bool Handled { get; set; }
	Keys KeyData { get; }
	Keys KeyCode { get; }
	Key  Key { get; }
	int KeyValue { get; }
	bool Alt { get; }
	bool Control { get; }
	bool Shift { get; }
	bool NoFurtherProcess { get; set; }
	void DisableVirtualKeyHandlingInThisEvent();
	int           ScanCode         { get; }
	bool          IsVirtual        { get; }
	int           Timestamp        { get; }
	bool          IsKeyDown        { get; }
	KeyEvent KeyEvent { get; }
	IKeyboardState KeyboardState { get; }
	IKeyPath PathToGo { get; }
	bool IsKeyUp { get; }
	bool IsExtendedKey { get; }
	IKeyEventArgs LastKeyDownEvent { get; }
	IKeyEventArgs LastKeyEvent     { get; }

	IKeyEventArgs LastKeyDownEvent_NoneVirtual { get; }
	IKeyEventArgs LastKeyEvent_NoneVirtual     { get; }
	void BeginInvoke(Action<IKeyEventArgs> action, DispatcherPriority priority = DispatcherPriority.Send);
	void BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Send);
	bool IsActive(ISequenceUnit hotKey)
	{
		var comb = hotKey.ToCombination();
		return comb.TriggerKey == KeyCode && comb.Chord.All(KeyboardState.IsDown);
	}
}