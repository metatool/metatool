using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Metatool.WindowsInput.Native;

namespace Metatool.WindowsInput;

/// <summary>
/// Implements the <see cref="IKeyboardSimulator"/> interface by calling the an <see cref="IInputMessageDispatcher"/> to simulate Keyboard gestures.
/// </summary>
public class KeyboardSimulator : IKeyboardSimulator
{
	private readonly IInputSimulator inputSimulator;

	/// <summary>
	/// The instance of the <see cref="IInputMessageDispatcher"/> to use for dispatching <see cref="INPUT"/> messages.
	/// </summary>
	private readonly IInputMessageDispatcher _messageDispatcher;

	/// <summary>
	/// Initializes a new instance of the <see cref="KeyboardSimulator"/> class using an instance of a <see cref="WindowsInputMessageDispatcher"/> for dispatching <see cref="INPUT"/> messages.
	/// </summary>
	/// <param name="inputSimulator">The <see cref="IInputSimulator"/> that owns this instance.</param>
	public KeyboardSimulator(IInputSimulator inputSimulator)
	{
		if (inputSimulator == null) throw new ArgumentNullException("inputSimulator");

		this.inputSimulator = inputSimulator;
		_messageDispatcher = new WindowsInputMessageDispatcher();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="KeyboardSimulator"/> class using the specified <see cref="IInputMessageDispatcher"/> for dispatching <see cref="INPUT"/> messages.
	/// </summary>
	/// <param name="inputSimulator">The <see cref="IInputSimulator"/> that owns this instance.</param>
	/// <param name="messageDispatcher">The <see cref="IInputMessageDispatcher"/> to use for dispatching <see cref="INPUT"/> messages.</param>
	/// <exception cref="InvalidOperationException">If null is passed as the <paramref name="messageDispatcher"/>.</exception>
	internal KeyboardSimulator(IInputSimulator inputSimulator, IInputMessageDispatcher messageDispatcher)
	{
		if (inputSimulator == null) throw new ArgumentNullException("inputSimulator");

		if (messageDispatcher == null)
			throw new InvalidOperationException(
				string.Format("The {0} cannot operate with a null {1}. Please provide a valid {1} instance to use for dispatching {2} messages.",
					typeof(KeyboardSimulator).Name, typeof(IInputMessageDispatcher).Name, typeof(INPUT).Name));

		this.inputSimulator = inputSimulator;
		_messageDispatcher = messageDispatcher;
	}

	/// <summary>
	/// Gets the <see cref="IMouseSimulator"/> instance for simulating Mouse input.
	/// </summary>
	/// <value>The <see cref="IMouseSimulator"/> instance.</value>
	public IMouseSimulator Mouse => inputSimulator.Mouse;

	private static void ModifiersDown(InputBuilder builder, IEnumerable<VirtualKeyCode> modifierKeyCodes)
	{
		if (modifierKeyCodes == null) return;
		foreach (var key in modifierKeyCodes) builder.AddKeyDown(key);
	}

	private static void ModifiersUp(InputBuilder builder, IEnumerable<VirtualKeyCode> modifierKeyCodes)
	{
		if (modifierKeyCodes == null) return;
		foreach (var key in modifierKeyCodes.Reverse()) builder.AddKeyUp(key);
	}

	private void KeysPress(InputBuilder builder, IEnumerable<VirtualKeyCode> keyCodes)
	{
		if (keyCodes == null) return;
		foreach (var key in keyCodes) builder.AddKeyPress(key);
	}

	/// <summary>
	/// Sends the list of <see cref="INPUT"/> messages using the <see cref="IInputMessageDispatcher"/> instance.
	/// </summary>
	/// <param name="inputList">The <see cref="System.Array"/> of <see cref="INPUT"/> messages to send.</param>
	private void SendSimulatedInput(INPUT[] inputList)
	{
		_messageDispatcher.DispatchInput(inputList);
	}

	/// <summary>
	/// Calls the Win32 SendInput method to simulate a KeyDown.
	/// </summary>
	/// <param name="keyCode">The <see cref="VirtualKeyCode"/> to press</param>
	public IKeyboardSimulator KeyDown(VirtualKeyCode keyCode)
	{
		var inputList = new InputBuilder().AddKeyDown(keyCode).ToArray();
		SendSimulatedInput(inputList);
		return this;
	}

	/// <summary>
	/// Calls the Win32 SendInput method to simulate a KeyUp.
	/// </summary>
	/// <param name="keyCode">The <see cref="VirtualKeyCode"/> to lift up</param>
	public IKeyboardSimulator KeyUp(VirtualKeyCode keyCode)
	{
		var inputList = new InputBuilder().AddKeyUp(keyCode).ToArray();
		SendSimulatedInput(inputList);
		return this;
	}

	/// <summary>
	/// Calls the Win32 SendInput method with a KeyDown and KeyUp message in the same input sequence in order to simulate a Key PRESS.
	/// </summary>
	/// <param name="keyCode">The <see cref="VirtualKeyCode"/> to press</param>
	public IKeyboardSimulator KeyPress(VirtualKeyCode keyCode)
	{
		var inputList = new InputBuilder().AddKeyPress(keyCode).ToArray();
		SendSimulatedInput(inputList);
		return this;
	}

	/// <summary>
	/// Simulates a key press for each of the specified key codes in the order they are specified.
	/// </summary>
	/// <param name="keyCodes"></param>
	public IKeyboardSimulator KeyPress(params VirtualKeyCode[] keyCodes)
	{
		var builder = new InputBuilder();
		KeysPress(builder, keyCodes);
		SendSimulatedInput(builder.ToArray());
		return this;
	}

	/// <summary>
	/// Simulates a simple modified keystroke like CTRL-C where CTRL is the modifierKey and C is the key.
	/// The flow is Modifier KeyDown, Key Press, Modifier KeyUp.
	/// </summary>
	/// <param name="modifierKeyCode">The modifier key</param>
	/// <param name="keyCode">The key to simulate</param>
	public IKeyboardSimulator ModifiedKeyStroke(VirtualKeyCode modifierKeyCode, VirtualKeyCode keyCode)
	{
		ModifiedKeyStroke(new[] { modifierKeyCode }, new[] { keyCode });
		return this;
	}

	/// <summary>
	/// Simulates a modified keystroke where there are multiple modifiers and one key like CTRL-ALT-C where CTRL and ALT are the modifierKeys and C is the key.
	/// The flow is Modifiers KeyDown in order, Key Press, Modifiers KeyUp in reverse order.
	/// </summary>
	/// <param name="modifierKeyCodes">The list of modifier keys</param>
	/// <param name="keyCode">The key to simulate</param>
	public IKeyboardSimulator ModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, VirtualKeyCode keyCode)
	{
		ModifiedKeyStroke(modifierKeyCodes, new[] { keyCode });
		return this;
	}

	/// <summary>
	/// Simulates a modified keystroke where there is one modifier and multiple keys like CTRL-K-C where CTRL is the modifierKey and K and C are the keys.
	/// The flow is Modifier KeyDown, Keys Press in order, Modifier KeyUp.
	/// </summary>
	/// <param name="modifierKey">The modifier key</param>
	/// <param name="keyCodes">The list of keys to simulate</param>
	public IKeyboardSimulator ModifiedKeyStroke(VirtualKeyCode modifierKey, IEnumerable<VirtualKeyCode> keyCodes)
	{
		ModifiedKeyStroke(new[] { modifierKey }, keyCodes);
		return this;
	}

	/// <summary>
	/// Simulates a modified keystroke where there are multiple modifiers and multiple keys like CTRL-ALT-K-C where CTRL and ALT are the modifierKeys and K and C are the keys.
	/// The flow is Modifiers KeyDown in order, Keys Press in order, Modifiers KeyUp in reverse order.
	/// </summary>
	/// <param name="modifierKeyCodes">The list of modifier keys</param>
	/// <param name="keyCodes">The list of keys to simulate</param>
	public IKeyboardSimulator ModifiedKeyStroke(IEnumerable<VirtualKeyCode> modifierKeyCodes, IEnumerable<VirtualKeyCode> keyCodes)
	{
		var builder = new InputBuilder();
		ModifiersDown(builder, modifierKeyCodes);
		KeysPress(builder, keyCodes);
		ModifiersUp(builder, modifierKeyCodes);

		SendSimulatedInput(builder.ToArray());
		return this;
	}

	public IKeyboardSimulator ModifiedKeyDown(IEnumerable<VirtualKeyCode> modifierKeyCodes, IEnumerable<VirtualKeyCode> keyCodes)
	{
		var builder = new InputBuilder();
		ModifiersDown(builder, modifierKeyCodes);
		foreach (var key in keyCodes) builder.AddKeyDown(key);
		SendSimulatedInput(builder.ToArray());
		return this;

	}

	public IKeyboardSimulator ModifiedKeyDown(IEnumerable<VirtualKeyCode> modifierKeyCodes, VirtualKeyCode keyCode)
	{
		return ModifiedKeyDown(modifierKeyCodes, new List<VirtualKeyCode>() {keyCode});
	}

	public IKeyboardSimulator ModifiedKeyUp(IEnumerable<VirtualKeyCode> modifierKeyCodes, VirtualKeyCode keyCode)
	{
		return ModifiedKeyUp(modifierKeyCodes, new List<VirtualKeyCode>() {keyCode});
	}
	public IKeyboardSimulator ModifiedKeyUp(IEnumerable<VirtualKeyCode> modifierKeyCodes, IEnumerable<VirtualKeyCode> keyCodes)
	{
		var builder = new InputBuilder();
		foreach (var key in keyCodes) builder.AddKeyUp(key);
		ModifiersUp(builder, modifierKeyCodes);
		SendSimulatedInput(builder.ToArray());
		return this;
	}
	class ToggleKeyCarer : IDisposable
	{
		private readonly VirtualKeyCode _key;
		private readonly InputBuilder _inputBuilder;
		private readonly IInputDeviceStateAdaptor _inputDeviceState = new WindowsInputDeviceStateAdaptor();
		readonly bool _isCapsLockToggled = false;
		public ToggleKeyCarer(VirtualKeyCode key, InputBuilder inputBuilder)
		{
			_key = key;
			_inputBuilder = inputBuilder;

			if (_inputDeviceState.IsToggleKeyOn(key))
			{
				_isCapsLockToggled = true;
				inputBuilder.AddKeyPress(key);
			}
		}

		public void Dispose()
		{
			if (_isCapsLockToggled) _inputBuilder.AddKeyPress(_key);
		}

	}

	/// <summary>
	/// Calls the Win32 SendInput method with a stream of KeyDown and KeyUp messages in order to simulate uninterrupted text entry via the keyboard.
	/// </summary>
	/// <param name="text">The text to be simulated.</param>
	public IKeyboardSimulator Type(string text, bool takeCareOfCapsLock = false)
	{


		if (text.Length > UInt32.MaxValue / 2) throw new ArgumentException(string.Format("The text parameter is too long. It must be less than {0} characters.", UInt32.MaxValue / 2), "text");


		var inputBuilder = new InputBuilder();

		if (takeCareOfCapsLock)
		{
			using (var capsLockCare = new ToggleKeyCarer(VirtualKeyCode.CAPITAL, inputBuilder))
			{
				inputBuilder = inputBuilder.AddCharacters(text);
			}
		}
		else
		{
			inputBuilder = inputBuilder.AddCharacters(text);
		}

		SendSimulatedInput(inputBuilder.ToArray());
		return this;
	}

	/// <summary>
	/// Simulates a single character text entry via the keyboard.
	/// </summary>
	/// <param name="character">The unicode character to be simulated.</param>
	public IKeyboardSimulator Type(char character)
	{
		var inputList = new InputBuilder().AddCharacter(character).ToArray();
		SendSimulatedInput(inputList);
		return this;
	}

	/// <summary>
	/// Sleeps the executing thread to create a pause between simulated inputs.
	/// </summary>
	/// <param name="millsecondsTimeout">The number of milliseconds to wait.</param>
	public IKeyboardSimulator Sleep(int millsecondsTimeout)
	{
		Thread.Sleep(millsecondsTimeout);
		return this;
	}

	/// <summary>
	/// Sleeps the executing thread to create a pause between simulated inputs.
	/// </summary>
	/// <param name="timeout">The time to wait.</param>
	public IKeyboardSimulator Sleep(TimeSpan timeout)
	{
		Thread.Sleep(timeout);
		return this;
	}

	/// <summary>
	/// Presses the given keys and releases them when the returned object is disposed.
	/// using (Keyboard.Pressing(VirtualKeyCode.CONTROL))
	/// {
	///    Keyboard.Type(VirtualKeyCode.VK_E);
	/// }
	/// </summary>
	public static IDisposable Pressing(params VirtualKeyCode[] virtualKeys)
	{
		return new KeyPressingActivation(virtualKeys);
	}

	private class KeyPressingActivation : IDisposable
	{
		private readonly VirtualKeyCode[] _virtualKeys;
		private readonly WindowsInputMessageDispatcher _messageDispatcher = new();
		private readonly InputBuilder _builder = new();

		public KeyPressingActivation(VirtualKeyCode[] virtualKeys)
		{
			_virtualKeys = virtualKeys;
			ModifiersDown(_builder, _virtualKeys);
			_messageDispatcher.DispatchInput(_builder.ToArray());
		}

		public void Dispose()
		{
			_builder.Clear();
			ModifiersUp(_builder, _virtualKeys);
			_messageDispatcher.DispatchInput(_builder.ToArray());
		}
	}
}