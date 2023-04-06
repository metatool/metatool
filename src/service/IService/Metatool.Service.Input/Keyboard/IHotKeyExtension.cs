using System;
using System.Threading.Tasks;
using Metatool.Command;
using Metatool.Service.Internal;

namespace Metatool.Service;

public static class IHotKeyExtension
{
	private static IKeyboard _keyboard;
	private static IKeyboard Keyboard =>
		_keyboard ??= Services.Get<IKeyboard>();

	private static ICommandManager _commandManager;
	private static ICommandManager CommandManager =>
		_commandManager ??= Services.Get<ICommandManager>();

	public static IKeyCommand Register(this IKeyboardCommandTrigger  trigger, Action<IKeyEventArgs> execute,
		Predicate<IKeyEventArgs> canExecute = null, string description = "")
	{
		var token            = CommandManager.Add(trigger, execute, canExecute, description);
		var keyboardInternal = (IKeyboardInternal)Keyboard;
		return keyboardInternal.GetToken(token, trigger);
	}

	public static IKeyCommand OnDown(this IHotkey hotkey, Action<IKeyEventArgs> execute,
		Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
	{
		var trigger = Keyboard.OnDown(hotkey, stateTree);
		return trigger.Register(execute, canExecute, description);
	}

	public static IKeyCommand OnUp(this IHotkey hotkey, Action<IKeyEventArgs> execute,
		Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
	{
		var trigger = Keyboard.OnUp(hotkey, stateTree);
		return trigger.Register(execute, canExecute, description);
	}
	public static IKeyCommand OnAllUp(this IHotkey sequenceUnit, Action<IKeyEventArgs> execute,
		Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
	{
		var trigger          = Keyboard.OnAllUp(sequenceUnit, stateTree);
		return trigger.Register(execute, canExecute, description);
	}
	public static IKeyCommand OnHit(this IHotkey sequenceUnit, Action<IKeyEventArgs> execute,
		Predicate<IKeyEventArgs> canExecute=null, string description="", string stateTree = KeyStateTrees.Default)
	{
		var trigger          = Keyboard.OnHit(sequenceUnit, stateTree);
		return trigger.Register(execute, canExecute, description);
	}
	public static IKeyCommand MapOnHit(this IHotkey key, ISequenceUnit target,
		Predicate<IKeyEventArgs> canExecute = null)
	{
		return Keyboard.MapOnHit(key, target, canExecute);
	}
	public static IKeyCommand MapOnAllUp(this IHotkey key, ISequenceUnit target,
		Predicate<IKeyEventArgs> canExecute = null)
	{
		return Keyboard.MapOnHitAndAllUp(key, target, canExecute);
	}
	public static IKeyCommand MapOnDownUp(this IHotkey key, ISequenceUnit target, Predicate<IKeyEventArgs> canExecute = null)
	{
		return Keyboard.MapOnDownUp(key, target, canExecute);
	}
	public static IKeyCommand HardMap(this IHotkey key, Key target, Predicate<IKeyEventArgs> canExecute = null)
	{
		return Keyboard.HardMap(key, new Combination(target), canExecute);
	}
	/// <summary>
	/// register the key to the state tree, and wait the down event;
	/// timeout: return null
	/// </summary>
	public static Task<IKeyEventArgs> DownAsync(this IHotkey hotkey, int timeout = -1, string description = "", string stateTree = KeyStateTrees.Default)
	{
		var command = new KeyEventAsync();
		hotkey.OnDown(command.OnEvent, null, description, stateTree);
		return command.WaitAsync(timeout);
	}

	public static Task<IKeyEventArgs> UpAsync(this IHotkey sequenceUnit, int timeout = -1, string description = "", string stateTree = KeyStateTrees.Default)
	{
		var command = new KeyEventAsync();
		sequenceUnit.OnUp(command.OnEvent, null, description, stateTree);
		return command.WaitAsync(timeout);
	}

	// if the handler async run, this is needed.
	public static IHotkey Handled(this IHotkey hotkey,
		KeyEvent keyEvent = KeyEvent.All)
	{
		hotkey.Handled = keyEvent;
		// if ((keyEvent & KeyEvent.Down) == KeyEvent.Down)
		//     hotkey.Down(e => e.Handled = true);
		// if ((keyEvent & KeyEvent.Up) == KeyEvent.Up)
		//     hotkey.Up(e => e.Handled = true);
		// if ((keyEvent & KeyEvent.AllUp) == KeyEvent.AllUp)
		//     hotkey.AllUp(e => e.Handled = true);
		return hotkey;
	}

}