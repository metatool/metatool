using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Metatool.Service;

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
		var chords = new List<Keys>();
		if(ctrl) chords.Add(Keys.ControlKey);
		if (shift) chords.Add(Keys.ShiftKey);
		if (alt) chords.Add(Keys.Menu);

		return new Combination((Keys)vkCode, chords);

	}
}