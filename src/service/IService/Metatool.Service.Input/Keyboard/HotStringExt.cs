using System;
using Metatool.Command;

namespace Metatool.Service;

public static class HotStringExt
{
	private static IKeyboard _keyboard;
	private static IKeyboard Keyboard =>
		_keyboard ??= Services.Get<IKeyboard>();
	public static IKeyCommand HotString(this string source, string target, Predicate<IKeyEventArgs> predicate = null)
	{
		return Keyboard.HotString(source, target, e=> !e.IsVirtual);
	}
}