using System;

namespace Metatool.Service.MouseKey;

public class Hotkey
{
	public static IHotkey Parse(string hotkey )
	{
		Key.TryParse(hotkey, out var key,false);
		if (key != null) return key;

		Combination.TryParse(hotkey, out var comb,false);
		if (comb != null) return comb;

		Sequence.TryParse(hotkey, out var seq);
		if (seq != null) return seq;

		throw new ArgumentException($"{hotkey} could not be parsed to IHotKey");
	}
}