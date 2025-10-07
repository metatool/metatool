using Metatool.Service.Internal;
using Metatool.Service.MouseKey;

namespace Metatool.Service;

public class ToggleKeys
{
	private static IKeyboardInternal _keyboard;

	private static IKeyboardInternal Keyboard =>
		_keyboard ??= (IKeyboardInternal)Services.Get<IKeyboard, IKeyboard>();

	public static IToggleKey NumLock    = Keyboard.GetToggleKey(Key.Num);
	public static IToggleKey CapsLock   = Keyboard.GetToggleKey(Key.Caps);
	public static IToggleKey ScrollLock = Keyboard.GetToggleKey(Key.Scroll);
	public static IToggleKey Insert     = Keyboard.GetToggleKey(Key.Ins);
}