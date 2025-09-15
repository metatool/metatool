using System.Collections.Generic;
using System.Windows.Forms;

namespace Metatool.Service;

public static class KeysExtensions
{
	private static IKeyboard _keyboard;
	private static IKeyboard Keyboard =>
		_keyboard ??= Services.Get<IKeyboard>();
	public static ICombination With(this Key key, KeyValues chord)
	{
		return new Combination(key, chord);
	}

	public static ICombination With(this Key triggerKey, IEnumerable<Key> chordsKeys)
	{
		return new Combination(triggerKey, chordsKeys);
	}

	public static bool IsDown(this Key key)
	{
		return Keyboard.IsDown(key);
	}
	public static bool IsUp(this Key key)
	{
		return Keyboard.IsUp(key);
	}
	public static bool IsToggled(this Key key)
	{
		return Keyboard.IsToggled(key);
	}
}