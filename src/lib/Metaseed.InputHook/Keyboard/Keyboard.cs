using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WindowsInput.Native;
using Metaseed.Input.implementation;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class Keyboard
    {
        static readonly KeyboardHook _Hook = new KeyboardHook();

        internal static IDisposable Add(ICombination combination, KeyAction action)
        {
            return _Hook.Add(combination, action);
        }

        internal static IDisposable Add(IList<ICombination> combinations, KeyAction action)
        {
            return _Hook.Add(combinations, action);
        }

        internal static IDisposable Map(Combination source, ICombination target, Predicate<ICombination> predicate = null)
        {
            var handled = false;
            return new Disposables()
            {
                 source.Down("", "", e =>
                {
                    if (predicate == null || predicate(source))
                    {
                        handled = true;
                        InputSimu.Inst.Keyboard.ModifiedKeyStroke(target.Chord.Cast<VirtualKeyCode>(),
                            (VirtualKeyCode) target.TriggerKey);
                        e.Handled = true;
                        return;
                    }

                    handled = false;
                }),
                source.Up("", "", e =>
                {
                    if (handled)
                    {
                        e.Handled = true;
                    }
                })
            };

        }
        public static event KeyPressEventHandler KeyPress
        {
            add => _Hook.EventSource.KeyPress    += value;
            remove => _Hook.EventSource.KeyPress -= value;
        }

        public static void HotKey(this string keys, string actionId, string description, Action action)
        {
            if (keys.Contains(','))
            {
                var sequence = Sequence.FromString(keys).ToList<ICombination>();
                Add(sequence, new KeyAction(actionId, description, e => action()));
            }

            var combination = Combination.FromString(keys) as Combination;
            Add(combination, new KeyAction(actionId, description, e => action()));
        }

        public static void Send(Keys key)
        {
            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)key);

        }

        public static void Hook()
        {
            _Hook.Run();
        }
    }
}
