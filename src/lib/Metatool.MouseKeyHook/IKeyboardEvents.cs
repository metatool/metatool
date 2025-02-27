﻿

using System.Windows.Forms;

namespace Metatool.Input.MouseKeyHook;

public interface IEvents
{
	bool Disable { get; set; }

}
/// <summary>
///     Provides keyboard events
/// </summary>
public interface IKeyboardEvents: IEvents
{
	/// <summary>
	///     Occurs when a key is pressed.
	/// </summary>
	event KeyEventHandler KeyDown;

	/// <summary>
	///     Occurs when a key is pressed.
	/// </summary>
	/// <remarks>
	///     Key events occur in the following order:
	///     <list type="number">
	///         <item>KeyDown</item>
	///         <item>KeyPress</item>
	///         <item>KeyUp</item>
	///     </list>
	///     The KeyPress event is not raised by non-character keys; however, the non-character keys do raise the KeyDown and
	///     KeyUp events.
	///     Use the KeyChar property to sample keystrokes at run time and to consume or modify a subset of common keystrokes.
	///     To handle keyboard events only in your application and not enable other applications to receive keyboard events,
	///     set the <see cref="KeyPressEventArgs.Handled" /> property in your form's KeyPress event-handling method to
	///     <b>true</b>.
	/// </remarks>
	event KeyPressEventHandler KeyPress;

	/// <summary>
	///     Occurs when a key is released.
	/// </summary>
	event KeyEventHandler KeyUp;
	bool HandleVirtualKey { get; set; }
	bool DisableDownEvent  { get; set; }
	bool DisableUpEvent    { get; set; }
	bool DisablePressEvent { get; set; }
}