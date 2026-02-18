using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Metatool.Input.MouseKeyHook.WinApi;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;

namespace Metatool.Input.MouseKeyHook.Implementation;

internal abstract class KeyListener(Subscribe subscribe) : BaseListener(subscribe), IKeyboardEvents
{
	private readonly ILogger _logger = Services.GetOrNull<ILogger<KeyListener>>();

	public event KeyEventHandler KeyDown;
	public event KeyPressEventHandler KeyPress;
	public event KeyEventHandler KeyUp;

	public void InvokeKeyDown(IKeyEventArgs e)
	{
		var handler = KeyDown;
		if (DisableDownEvent)
		{
			_logger?.LogDebug("this KeyUp event disabled");
			return;
		}
		// Debug.WriteLine($"InvokeKeyDown: {e}");
		if (handler == null || !e.IsKeyDown || e.Handled)
			return;

		handler(this, e);
	}

	public void InvokeKeyPress(KeyPressEventArgsExt e)
	{
		var handler = KeyPress;
		if (DisablePressEvent)
		{
			_logger?.LogDebug("this KeyPress event disabled");
			return;
		}

		if (handler == null || e.Handled || e.IsNonChar)
			return;

		handler(this, e);
		_logger?.LogDebug(new string('\t', _indentCounter) + e);
	}

	public void InvokeKeyUp(IKeyEventArgs e)
	{
		var handler = KeyUp;

		if (handler == null || !e.IsKeyUp || e.Handled)
			return;

		if (KeyboardState.HandledDownKeys.IsDown(e.KeyCode))
		{
			KeyboardState.HandledDownKeys.SetKeyUp(e.KeyCode);
		}

		if (DisableUpEvent)
		{
			_logger?.LogDebug("this KeyUp event disabled");
			return;
		}

		handler(this, e);
	}

	private int _indentCounter = 0;

	public bool HandleVirtualKey { get; set; } = true;

	private bool _disableDownEvent;
	private bool _disableUpEvent;
	private bool _disablePressEvent;

	public bool DisableDownEvent
	{
		get => _disableDownEvent;

		set
		{
			_logger?.LogDebug($"{nameof(DisableDownEvent)} = {value}");
			_disableDownEvent = value;
		}
	}

	public bool DisableUpEvent
	{
		get => _disableUpEvent;

		set
		{
			_logger?.LogDebug($"{nameof(DisableUpEvent)} = {value}");
			_disableUpEvent = value;
		}
	}

	public bool DisablePressEvent
	{
		get => _disablePressEvent;
		set
		{
			_logger?.LogDebug($"{nameof(DisablePressEvent)} = {value}");
			_disablePressEvent = value;
		}
	}

	/// <returns>false: to block the system to continue processing in other hooks, i.e. prevent the key typing</returns>
	protected override bool Callback(CallbackData data)
	{
		var args = GetDownUpEventArgs(data);
		if (Disable)
		{
			_logger?.LogDebug('\t' + "KeyListener is disable, NotHandled: " + args);
			return true;
		}

		var argExt = args as KeyEventArgsExt;
		argExt.listener = this;
		if (args.IsVirtual && !HandleVirtualKey)
		{
			_logger?.LogDebug('\t' + "KeyListener configured to Not HandleVirtualKey, Not Handled " + args);
			return true;
		}

		_logger?.LogDebug(new string('\t', _indentCounter++) + "→" + args);
		// down
		InvokeKeyDown(args);
		// press
		var pressEventArgs = GetPressEventArgs(data, args).ToArray();

		foreach (var pressEventArg in pressEventArgs)
			InvokeKeyPress(pressEventArg);
		// up
		InvokeKeyUp(args);

		_logger?.LogDebug(new string('\t', --_indentCounter) + "←" + args);

		if (argExt.HandleVirtualKeyBackup.HasValue)
		{
			HandleVirtualKey = argExt.HandleVirtualKeyBackup.Value;
			_logger?.LogDebug($"HandleVirtualKey={HandleVirtualKey}");
		}

		return !args.Handled && !pressEventArgs.Any(e => e.Handled);
	}

	protected abstract IEnumerable<KeyPressEventArgsExt> GetPressEventArgs(CallbackData data, IKeyEventArgs arg);
	protected abstract IKeyEventArgs GetDownUpEventArgs(CallbackData data);
}