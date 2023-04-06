using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Service;

public static class StringExt
{
	private static IKeyboard _keyboard;
	private static IKeyboard Keyboard =>
		_keyboard ??= Services.Get<IKeyboard>();
	public static IHotkey ToHotkey(this string hotkey, params IDictionary<string, string>[] additionalAliasesDics)
	{
		hotkey = Keyboard.ReplaceAlias(hotkey, additionalAliasesDics);
		return Sequence.Parse(hotkey);
	}
}