using System.Windows.Threading;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public class ToggleKey : IToggleKey
{
	private readonly Key _key;
	private bool? _isAlwaysOn;
	private bool _confirmAlwaysOnOffSate;
	private IKeyCommand _keyCommandDownActionToken;
	private IKeyCommand _keyCommandUpActionToken;

	internal ToggleKey(Key key)
	{
		_key = key;
	}

	public ToggleKeyState State
	{
		get
		{
			if (_isAlwaysOn.HasValue) return _isAlwaysOn.Value ? ToggleKeyState.AlwaysOn : ToggleKeyState.AlwaysOff;

			return KeyboardState.Current().IsKeyLocked((KeyCodes)_key) ? ToggleKeyState.On : ToggleKeyState.Off;
		}
	}

	void InstallHook(string description)
	{
		bool handleViaSystem = false;

		if (_keyCommandDownActionToken == null)
			_keyCommandDownActionToken = _key.OnDown(e =>
			{
				if (!_isAlwaysOn.HasValue) return;

				if (_key == KeyCodes.NumLock)
				{
					var isOn = KeyboardState.Current().IsKeyLocked((KeyCodes)_key);
					if (isOn && !_isAlwaysOn.Value || !isOn && _isAlwaysOn.Value)
					{
						handleViaSystem = true;
					}
					// e.Handled is not set to true, so will toggle it via system logic.
					return;
				}

				if (_confirmAlwaysOnOffSate)
				{
					_confirmAlwaysOnOffSate = false;
					var isOn = KeyboardState.Current().IsKeyLocked((KeyCodes)_key);
					if (isOn && !_isAlwaysOn.Value || !isOn && _isAlwaysOn.Value)
					{
						handleViaSystem = true;
						return;
					}
				}

				// prevent system to toggle it
				e.Handled = true;
			}, e => !e.IsVirtual, description, stateTree: KeyStateTrees.Map);

		if (_keyCommandUpActionToken == null)
			_keyCommandUpActionToken = _key.OnUp(e =>
			{
				if (!_isAlwaysOn.HasValue) return;

				if (handleViaSystem)
				{
					handleViaSystem = false;
					// e.handled is not set to true, so will trigger system logic
					return;
				}

				if (_key == KeyCodes.NumLock)
				{
					IKeyboard.Inst.Send(() => IKeyboard.Inst.Type(_key));
					return;
				}

				e.Handled = true;
			}, e => !e.IsVirtual, description, stateTree: KeyStateTrees.Map);
	}

	void RemoveHook()
	{
		_keyCommandDownActionToken?.Remove();
		_keyCommandDownActionToken = null;
		_keyCommandUpActionToken?.Remove();
		_keyCommandUpActionToken = null;
	}

	public void AlwaysOn()
	{
		InstallHook($"Key: {_key} AlwaysOn");

		switch (State)
		{
			case ToggleKeyState.Off:
			case ToggleKeyState.AlwaysOff:
				_isAlwaysOn = true;
				_confirmAlwaysOnOffSate = true;
				// Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
				// 		InputSimu.Inst.Keyboard.KeyPress((KeyCodes)(KeyCodes)_key)
				//	));
				IKeyboard.Inst.Send(() => IKeyboard.Inst.Type(_key));
				break;
			case ToggleKeyState.On:
				_isAlwaysOn = true;
				break;
			case ToggleKeyState.AlwaysOn:
				break;
		}
	}
	public void AlwaysOff()
	{
		InstallHook($"Key: {_key} AlwaysOff");

		switch (State)
		{
			case ToggleKeyState.AlwaysOff:
				break;
			case ToggleKeyState.On:
			case ToggleKeyState.AlwaysOn:
				_confirmAlwaysOnOffSate = true;
				_isAlwaysOn = false;
				// Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
				// 		InputSimu.Inst.Keyboard.KeyPress((KeyCodes)(KeyCodes)_key)
				// 	));
				IKeyboard.Inst.Send(() => IKeyboard.Inst.Type(_key));

				break;
			case ToggleKeyState.Off:
				_isAlwaysOn = false;
				break;
		}

	}

	public void Off()
	{
		RemoveHook();
		switch (State)
		{
			case ToggleKeyState.Off:
				break;
			case ToggleKeyState.On:
				IKeyboard.Inst.Type(_key);
				break;
			case ToggleKeyState.AlwaysOff:
				_isAlwaysOn = null;
				break;
			case ToggleKeyState.AlwaysOn:
				_isAlwaysOn = null;
				IKeyboard.Inst.Type(_key);
				break;
		}

	}

	public void On()
	{
		RemoveHook();
		switch (State)
		{
			case ToggleKeyState.Off:
				IKeyboard.Inst.Type(_key);
				break;
			case ToggleKeyState.On:
				break;
			case ToggleKeyState.AlwaysOff:
				_isAlwaysOn = null;
				IKeyboard.Inst.Type(_key);
				break;
			case ToggleKeyState.AlwaysOn:
				_isAlwaysOn = null;
				break;
		}

	}

}