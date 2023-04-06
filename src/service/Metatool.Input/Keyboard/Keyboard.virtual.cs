using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Annotations;
using System.Windows.Forms;
using Metatool.Service;
using Metatool.WindowsInput.Native;

namespace Metatool.Input;

public partial class Keyboard
{
	public void Type(params IHotkey[] keys)
	{
		foreach (var key in keys)
		{
			switch (key)
			{
				case Key k:
					InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) (Keys) k);
					break;
				case ICombination combination:
					InputSimu.Inst.Keyboard.ModifiedKeyStroke(
						combination.Chord.Select(k => (VirtualKeyCode) (Keys) k),
						(VirtualKeyCode) (Keys) combination.TriggerKey);
					break;
				case ISequence sequence:
				{
					foreach (var comb in sequence)
					{
						InputSimu.Inst.Keyboard.ModifiedKeyStroke(comb.Chord.Cast<VirtualKeyCode>(),
							(VirtualKeyCode) (Keys) comb.TriggerKey);
					}

					break;
				}
				default:
					throw new ArgumentException($"unsupported type:{key.GetType()}, in Keyboard.Type method.");
			}
		}
	}

	public void Down(params IHotkey[] keys)
	{
		foreach (var key in keys)
		{
			switch (key)
			{
				case Key k:
					InputSimu.Inst.Keyboard.KeyDown((VirtualKeyCode) (Keys) k);
					break;
				case ICombination combination:
					InputSimu.Inst.Keyboard.ModifiedKeyDown(
						combination.Chord.Select(k => (VirtualKeyCode) (Keys) k),
						(VirtualKeyCode) (Keys) combination.TriggerKey);
					break;
				case ISequence sequence:
				{
					foreach (var comb in sequence)
					{
						InputSimu.Inst.Keyboard.ModifiedKeyDown(comb.Chord.Select(k => (VirtualKeyCode) (Keys) k),
							(VirtualKeyCode) (Keys) comb.TriggerKey);
					}

					break;
				}
				default:
					throw new ArgumentException($"unsupported type:{key.GetType()}, in Keyboard.Type method.");
			}
		}
	}
	public void Up(params IHotkey[] keys)
	{
		foreach (var key in keys)
		{
			switch (key)
			{
				case Key k:
					InputSimu.Inst.Keyboard.KeyUp((VirtualKeyCode) (Keys) k);
					break;
				case ICombination combination:
					InputSimu.Inst.Keyboard.ModifiedKeyUp(combination.Chord.Select(k => (VirtualKeyCode) (Keys) k),
						(VirtualKeyCode) (Keys) combination.TriggerKey);
					break;
				case ISequence sequence:
				{
					foreach (var comb in sequence)
					{
						InputSimu.Inst.Keyboard.ModifiedKeyUp(comb.Chord.Select(k => (VirtualKeyCode) (Keys) k),
							(VirtualKeyCode) (Keys) comb.TriggerKey);
					}

					break;
				}
				default:
					throw new ArgumentException($"unsupported type:{key.GetType()}, in Keyboard.Type method.");
			}
		}
	}

	public void Type(char character) => InputSimu.Inst.Keyboard.Type(character);

	public void Type(string text) => InputSimu.Inst.Keyboard.Type(text);
}