using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Metatool.Service;
using Metatool.WindowsInput.Native;

namespace Metatool.Input
{
    public partial class Keyboard
    {
        public void Type(IHotkey key)
        {
            switch (key)
            {
                case Key k:
                    InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) (Keys) k);
                    break;
                case ICombination combination:
                    InputSimu.Inst.Keyboard.ModifiedKeyStroke(combination.Chord.Cast<VirtualKeyCode>(),
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

        public void Down(IHotkey key)
        {
            switch (key)
            {
                case Key k:
                    InputSimu.Inst.Keyboard.KeyDown((VirtualKeyCode)(Keys)k);
                    break;
                case ICombination combination:
                    InputSimu.Inst.Keyboard.ModifiedKeyDown(combination.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode)(Keys)combination.TriggerKey);
                    break;
                case ISequence sequence:
                {
                    foreach (var comb in sequence)
                    {
                        InputSimu.Inst.Keyboard.ModifiedKeyDown(comb.Chord.Cast<VirtualKeyCode>(),
                            (VirtualKeyCode)(Keys)comb.TriggerKey);
                    }

                    break;
                }
                default:
                    throw new ArgumentException($"unsupported type:{key.GetType()}, in Keyboard.Type method.");
            }
        }
        public void Up(IHotkey key)
        {
            switch (key)
            {
                case Key k:
                    InputSimu.Inst.Keyboard.KeyUp((VirtualKeyCode)(Keys)k);
                    break;
                case ICombination combination:
                    InputSimu.Inst.Keyboard.ModifiedKeyUp(combination.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode)(Keys)combination.TriggerKey);
                    break;
                case ISequence sequence:
                {
                    foreach (var comb in sequence)
                    {
                        InputSimu.Inst.Keyboard.ModifiedKeyUp(comb.Chord.Cast<VirtualKeyCode>(),
                            (VirtualKeyCode)(Keys)comb.TriggerKey);
                    }

                    break;
                }
                default:
                    throw new ArgumentException($"unsupported type:{key.GetType()}, in Keyboard.Type method.");
            }
        }
        public void Type(IEnumerable<Key> keys) =>
            InputSimu.Inst.Keyboard.KeyPress(keys.Select(k => (VirtualKeyCode) (Keys) k).ToArray());

        public void Type(char character) => InputSimu.Inst.Keyboard.Type(character);

        public void Type(string text) => InputSimu.Inst.Keyboard.Type(text);
    }
}