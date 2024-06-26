﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Metatool.Service;

public partial class Sequence
{
	public ISequence Then(Keys key)
	{
		return this.Then(new Combination(key));
	}

	public ISequence Then(IHotkey hotkey)
	{
		this.AddRange(hotkey.ToSequence());
		return this;
	}

	public KeyEvent Handled
	{
		get => this.Last().TriggerKey.Handled;
		set => this.Last().TriggerKey.Handled = value;
	}
	public ISequence ToSequence() => this;
}