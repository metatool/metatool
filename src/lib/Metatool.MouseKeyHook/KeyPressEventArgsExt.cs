﻿// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service;

namespace Metatool.Input;

/// <summary>
///     Provides extended data for the <see cref='KeyListener.KeyPress' /> event.
/// </summary>
public class KeyPressEventArgsExt : KeyPressEventArgs, IKeyPressEventArgs
{
	internal KeyPressEventArgsExt(char keyChar, IKeyEventArgs arg, int timestamp)
		: base(keyChar)
	{
		IsNonChar = keyChar == (char) 0x0;
		Timestamp = timestamp;
		EventArgs = arg;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref='KeyPressEventArgsExt' /> class.
	/// </summary>
	/// <param name="keyChar">
	///     Character corresponding to the key pressed. 0 char if represents a system or functional non char
	///     key.
	/// </param>
	public KeyPressEventArgsExt(char keyChar,IKeyEventArgs arg)
		: this(keyChar, arg, Environment.TickCount)
	{
	}
	public IKeyEventArgs EventArgs { get; }

	public override string ToString()
	{
		var dt = DateTime.Now;
		dt = dt.AddMilliseconds(Timestamp - Environment.TickCount);
		var et = "Press";
		return $"{dt:hh:mm:ss.fff}  {KeyChar,-16}{et,-6}Handled:{Handled,-8} IsNoChar:{IsNonChar,-8} ";
	}
	/// <summary>
	///     True if represents a system or functional non char key.
	/// </summary>
	public bool IsNonChar { get; }

	/// <summary>
	///     The system tick count of when the event occurred.
	/// </summary>
	public int Timestamp { get; }

	internal static IEnumerable<KeyPressEventArgsExt> FromRawDataApp(CallbackData data, IKeyEventArgs arg)
	{
		var wParam = data.WParam;
		var lParam = data.LParam;

		//http://msdn.microsoft.com/en-us/library/ms644984(v=VS.85).aspx

		const uint maskKeydown = 0x40000000; // for bit 30
		const uint maskKeyup = 0x80000000; // for bit 31
		const uint maskScanCode = 0xff0000; // for bit 23-16

		var flags = (uint) lParam.ToInt64();

		//bit 30 Specifies the previous key state. The value is 1 if the key is down before the message is sent; it is 0 if the key is up.
		var wasKeyDown = (flags & maskKeydown) > 0;
		//bit 31 Specifies the transition state. The value is 0 if the key is being pressed and 1 if it is being released.
		var isKeyReleased = (flags & maskKeyup) > 0;

		if (!wasKeyDown && !isKeyReleased)
			yield break;

		var virtualKeyCode = (int) wParam;
		var scanCode = checked((int) (flags & maskScanCode));
		const int fuState = 0;

		char[] chars;

		KeyboardNativeMethods.TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, out chars);
		if (chars == null) yield break;
		foreach (var ch in chars)
			yield return new KeyPressEventArgsExt(ch,arg);
	}

	internal static IEnumerable<KeyPressEventArgsExt> FromRawDataGlobal(CallbackData data, IKeyEventArgs arg)
	{
		var wParam = data.WParam;
		var lParam = data.LParam;

		if ((int) wParam != Messages.WM_KEYDOWN && (int) wParam != Messages.WM_SYSKEYDOWN)
			yield break;

		var keyboardHookStruct =
			(KeyboardHookStruct) Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

		var virtualKeyCode = keyboardHookStruct.VirtualKeyCode;
		var scanCode = keyboardHookStruct.ScanCode;
		var fuState = keyboardHookStruct.Flags;

		if (virtualKeyCode == KeyboardNativeMethods.VK_PACKET)
		{
			var ch = (char) scanCode;
			yield return new KeyPressEventArgsExt(ch, arg, keyboardHookStruct.Time);
		}
		else
		{
			char[] chars;
			KeyboardNativeMethods.TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, out chars);
			if (chars == null) yield break;
			foreach (var current in chars)
				yield return new KeyPressEventArgsExt(current,arg, keyboardHookStruct.Time);
		}
	}
}