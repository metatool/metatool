using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WindowsInput.Native;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;
using System.Windows.Threading;

namespace Metaseed.Input
{
    public static class Keyboard
    {
        static readonly KeyboardHook _Hook = new KeyboardHook();
        static Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        internal static IRemovable Add(ICombination combination, KeyAction action)
        {
            return _Hook.Add(combination, action);
        }

        internal static IRemovable Add(IList<ICombination> combinations, KeyAction action)
        {
            return _Hook.Add(combinations, action);
        }

        internal static IRemovable Map(Combination source, ICombination target, Predicate<KeyEventArgsExt> predicate = null)
        {
            var handled = false;
            return new Removables()
            {
                 source.Down($"Map_Down_{source}_To_{target}", "", e =>
                {
                    if (predicate == null || predicate(e))
                    {
                        handled = true;
                        KeyboardState.OnToggleKeys.Add(source.TriggerKey);
                        e.Handled = true;
                        if (target.TriggerKey == Keys.LButton)
                        {
                           Async(() => InputSimu.Inst.Mouse.LeftButtonDown());
                            return;
                        } else if (target.TriggerKey == Keys.RButton)
                        {
                            Async(() => InputSimu.Inst.Mouse.RightButtonDown());
                            return;
                        }
                        Async(() => InputSimu.Inst.Keyboard.ModifiedKeyDown(target.Chord.Cast<VirtualKeyCode>(),
                                (VirtualKeyCode) target.TriggerKey));
                        return;
                    }

                    handled = false;
                }),
                source.Up("Map_Up_{source}_To_{target}", "", e =>
                {
                    if (handled)
                    {
                        handled = false;
                        KeyboardState.OnToggleKeys.Remove(source.TriggerKey);
                        if (predicate == null || predicate(e))
                        {
                            e.Handled = true;
                            if (target.TriggerKey == Keys.LButton)
                            {
                                Async(() => InputSimu.Inst.Mouse.LeftButtonUp());
                                return;
                            } else if (target.TriggerKey == Keys.RButton)
                            {
                               Async( () => InputSimu.Inst.Mouse.RightButtonUp());
                                return;
                            }
                           Async(() => InputSimu.Inst.Keyboard.ModifiedKeyUp(
                                    target.Chord.Cast<VirtualKeyCode>(),
                                    (VirtualKeyCode) target.TriggerKey));
                        }


                    }
                })
            };

        }
        internal static IRemovable MapOnHit(Combination source, ICombination target, Predicate<KeyEventArgsExt> predicate = null)
        {
            var handled = false;
            KeyEventArgsExt lastKeyDown = null;
            return new Removables()
            {
                source.Down($"MapOnHit_Down_{source}_To_{target}", "", e =>
                {
                    if (predicate == null || predicate(e))
                    {
                        handled = true;
                        KeyboardState.OnToggleKeys.Add(source.TriggerKey);
                        e.Handled = true;
                        lastKeyDown = e;
                        return;
                    }

                    handled = false;
                }),
                source.Up("MapOnHit_Up_{source}_To_{target}", "", e =>
                {
                    if (handled)
                    {
                        handled = false;
                        KeyboardState.OnToggleKeys.Remove(source.TriggerKey);
                        if (lastKeyDown == e.LastKeyDownEvent && (predicate == null || predicate(e)))
                        {
                            e.Handled = true;
                            Async(()=> InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                                    target.Chord.Cast<VirtualKeyCode>(),
                                    (VirtualKeyCode) target.TriggerKey));
                        }

                    }
                })
            };

        }
        public static event KeyPressEventHandler KeyPress
        {
            add => _Hook.EventSource.KeyPress += value;
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

        private static void Async(Action action, DispatcherPriority priority = DispatcherPriority.Send)
        {
            _dispatcher.BeginInvoke(priority, action);
        }
        public static void Type(Keys key, bool IsAsync)
        {
            if (IsAsync)
                Async(() => InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)key));
            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)key);
        }

        public static void Type(Keys[] keys) => InputSimu.Inst.Keyboard.KeyPress(keys.Cast<VirtualKeyCode>().ToArray());


        public static void Type(char character) => InputSimu.Inst.Keyboard.Type(character);

        public static void Type(string text) => InputSimu.Inst.Keyboard.Type(text);

        public static void Hook()
        {
            _Hook.Run();
        }
    }
}
