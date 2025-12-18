using System;
using System.Linq;

namespace Metatool.Service.MouseKey;

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
	Key  Key { get; }
    KeyCodes KeyData { get; }
    KeyCodes KeyCode { get; }

    bool Alt { get; }
	bool Control { get; }
	bool Shift { get; }
	bool NoFurtherProcess { get; set; }
	void DisableVirtualKeyHandlingInThisEvent();
	int           ScanCode         { get; }
	bool          IsVirtual        { get; }
	int           Timestamp        { get; }
	bool          IsKeyDown        { get; }
	KeyEventType KeyEventType { get; set; }
	IKeyboardState KeyboardState { get; }
	IKeyPath PathToGo { get; }
	bool IsKeyUp { get; }
	bool IsExtendedKey { get; }
	IKeyEventArgs LastKeyDownEvent { get; }
	IKeyEventArgs LastKeyEvent     { get; }

	IKeyEventArgs LastKeyDownEvent_NoneVirtual { get; }
	IKeyEventArgs LastKeyEvent_NoneVirtual     { get; }
	void BeginInvoke(Action<IKeyEventArgs> action);
	void BeginInvoke(Action action);
	bool IsActive(ISequenceUnit hotKey)
	{
		var comb = hotKey.ToCombination();
		return comb.TriggerKey == KeyCode && comb.Chord.All(KeyboardState.IsDown);
	}
}