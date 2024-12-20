﻿using System.Windows.Forms;

namespace Metatool.Service;

public partial class Key
{
	public ISequence Then(Keys key) => new Combination(this).Then(key);

	public ISequence Then(IHotkey hotkey) => new Combination(this).Then(hotkey);

	public ISequence ToSequence() => this.ToCombination().ToSequence();

}