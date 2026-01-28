using System.Runtime.InteropServices;
using Metatool.Service.MouseKey;

namespace Metatool.Input.implementation;

internal class Helper
{
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	static extern short VkKeyScan(char ch);

	internal static ICombination CharToKey(char c)
	{
		var vkKeyScan = VkKeyScan(c);
		var vkCode    = vkKeyScan & 0xff;
		var shift     = (vkKeyScan & 0x100) > 0;
		var ctrl      = (vkKeyScan & 0x200) > 0;
		var alt       = (vkKeyScan & 0x400) > 0;
		var chords = new List<KeyCodes>();
		if(ctrl) chords.Add(KeyCodes.ControlKey);
		if (shift) chords.Add(KeyCodes.ShiftKey);
		if (alt) chords.Add(KeyCodes.Menu);

		return new Combination((KeyCodes)vkCode, chords);

	}
}