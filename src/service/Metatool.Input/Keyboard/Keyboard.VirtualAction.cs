using System;
using System.Linq;
using Metatool.Service.MouseKey;

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
                    InputSimu.Inst.Keyboard.KeyPress((KeyCodes)(KeyCodes)k);
                    break;
                case ICombination combination:
                    InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                        combination.Chord.Select(k => (KeyCodes)(KeyCodes)k),
                        (KeyCodes)(KeyCodes)combination.TriggerKey);
                    break;
                case ISequence sequence:
                    {
                        foreach (var comb in sequence)
                        {
                            InputSimu.Inst.Keyboard.ModifiedKeyStroke(comb.Chord.Cast<KeyCodes>(),
                                (KeyCodes)(KeyCodes)comb.TriggerKey);
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
                    InputSimu.Inst.Keyboard.KeyDown((KeyCodes)(KeyCodes)k);
                    break;
                case ICombination combination:
                    InputSimu.Inst.Keyboard.ModifiedKeyDown(
                        combination.Chord.Select(k => (KeyCodes)(KeyCodes)k),
                        (KeyCodes)(KeyCodes)combination.TriggerKey);
                    break;
                case ISequence sequence:
                    {
                        foreach (var comb in sequence)
                        {
                            InputSimu.Inst.Keyboard.ModifiedKeyDown(comb.Chord.Select(k => (KeyCodes)(KeyCodes)k),
                                (KeyCodes)(KeyCodes)comb.TriggerKey);
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
                    InputSimu.Inst.Keyboard.KeyUp((KeyCodes)(KeyCodes)k);
                    break;
                case ICombination combination:
                    InputSimu.Inst.Keyboard.ModifiedKeyUp(combination.Chord.Select(k => (KeyCodes)(KeyCodes)k),
                        (KeyCodes)(KeyCodes)combination.TriggerKey);
                    break;
                case ISequence sequence:
                    {
                        foreach (var comb in sequence)
                        {
                            InputSimu.Inst.Keyboard.ModifiedKeyUp(comb.Chord.Select(k => (KeyCodes)(KeyCodes)k),
                                (KeyCodes)(KeyCodes)comb.TriggerKey);
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