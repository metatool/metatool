using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;
using System.Windows.Threading;
using Metaseed.MetaKeyboard;
using Metaseed.WindowsInput.Native;
using OneOf;
using KeyEventHandler = Metaseed.Input.MouseKeyHook.KeyEventHandler;

namespace Metaseed.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public static class Keyboard
    {
        public static   IKeyPath     Root        = null;
        static readonly KeyboardHook _Hook       = new KeyboardHook();
        static          Dispatcher   _dispatcher = Dispatcher.CurrentDispatcher;

        internal static IMetaKey Add(ICombination combination, KeyEvent keyEvent, KeyCommand command,
            KeyStateTree stateTree = null)
        {
            return Add(new List<ICombination>() {combination}, keyEvent, command, stateTree);
        }

        internal static IMetaKey Add(IList<ICombination> combinations, KeyEvent keyEvent, KeyCommand command,
            KeyStateTree stateTree = null)
        {
            return _Hook.Add(combinations, new KeyEventCommand(keyEvent, command), stateTree);
        }

        public static void ShowTip()
        {
            _Hook.ShowTip();
        }

        public static void Hit(Keys key, IEnumerable<Keys> modifierKeys = null, bool isAsync = false)
        {
            if (isAsync)
            {
                Async(() => Hit(key, modifierKeys, false));
                return;
            }

            if (modifierKeys == null) InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);
            InputSimu.Inst.Keyboard.ModifiedKeyStroke(modifierKeys.Cast<VirtualKeyCode>(),
                (VirtualKeyCode) key);
        }

        private static Action Repeat(int repeat, Action action)
        {
            return () =>
            {
                while (repeat-- > 0) action();
            };
        }

        internal static IMetaKey HardMap(ICombination source, ICombination target,
            Predicate<KeyEventArgsExt> predicate = null)
        {
            var handled = false;
            return new MetaKeys()
            {
                source.Down(e =>
                {
                    handled            = true;
                    e.Handled          = true;
                    e.NoFurtherProcess = true;

                    InputSimu.Inst.Keyboard.ModifiedKeyDown(
                        target.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys) target.TriggerKey);
                }, predicate, "", KeyStateTree.HardMap),
                source.Up(e =>
                {
                    handled = false;

                    e.Handled          = true;
                    e.NoFurtherProcess = true;

                    InputSimu.Inst.Keyboard.ModifiedKeyUp(target.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys) target.TriggerKey);
                }, e =>
                {
                    if (!handled)
                    {
                        Console.WriteLine("\t/!Handling:false");
                        return false;
                    }

                    if (predicate != null && !predicate(e))
                    {
                        Console.WriteLine("\t/!predicate(e):false");
                        return false;
                    }

                    return true;
                }, "", KeyStateTree.HardMap)
            };
        }

        internal static IMetaKey Map(string source, string target, Predicate<KeyEventArgsExt> predicate = null)
        {
            var sequence = Sequence.FromString(string.Join(",", source.ToUpper().ToCharArray()));
            var send     = Enumerable.Repeat(Keys.Back, source.Length).Cast<VirtualKeyCode>();
            return sequence.Up(e =>
            {
                e.BeginInvoke(() =>
                    {
                        Notify.ShowSelectionAction(new[]
                        {
                            (target,
                                (Action) (() =>
                                    {
                                        InputSimu.Inst.Keyboard.KeyPress(send.ToArray());
                                        InputSimu.Inst.Keyboard.Type(target);
                                    }
                                ))
                        });
                    }
                );
            }, predicate, "", KeyStateTree.HotString);
        }


        internal static IMetaKey Map(ICombination source, ICombination target,
            Predicate<KeyEventArgsExt> predicate = null, int repeat = 1)
        {
            var handled = false;
            return new MetaKeys()
            {
                source.Down(e =>
                {
                    handled   = true;
                    e.Handled = true;

                    if (target.TriggerKey == Keys.LButton)
                    {
                        Async(Repeat(repeat, () => InputSimu.Inst.Mouse.LeftDown()));
                    }
                    else if (target.TriggerKey == Keys.RButton)
                    {
                        Async(Repeat(repeat, () => InputSimu.Inst.Mouse.RightDown()));
                    }
                    else
                    {
                        Async(Repeat(repeat, () => InputSimu.Inst.Keyboard.ModifiedKeyDown(
                            target.Chord.Cast<VirtualKeyCode>(),
                            (VirtualKeyCode) (Keys) target.TriggerKey)
                        ));
                    }
                }, predicate, "", KeyStateTree.Map),
                source.Up(e =>
                {
                    if (!handled) return;
                    handled   = false;
                    e.Handled = true;
                    if (target.TriggerKey == Keys.LButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.LeftUp());
                        return;
                    }
                    else if (target.TriggerKey == Keys.RButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.RightUp());
                        return;
                    }

                    InputSimu.Inst.Keyboard.ModifiedKeyUp(target.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys) target.TriggerKey);
                }, predicate, "", KeyStateTree.Map)
            };
        }

        internal static IMetaKey MapOnHit(ICombination source, ICombination target,
            Predicate<KeyEventArgsExt> predicate = null, bool allUp = true)
        {
            var             handling     = false;
            KeyEventArgsExt keyDownEvent = null;

            void AsyncCall(KeyEventArgsExt e)
            {
                e.Handled = true;
                e.BeginInvoke(() => InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                    target.Chord.Select(k => (VirtualKeyCode) (Keys) k),
                    (VirtualKeyCode) (Keys) target.TriggerKey));
            }

            // if not: A+B -> C become A+C
            bool KeyUpPredicate(KeyEventArgsExt e)
            {
                if (!handling)
                {
                    Console.WriteLine("\t/!Handling:false");
                    return false;
                }

                handling = false;

                if (!allUp && keyDownEvent != e.LastKeyDownEvent
                ) // should not use LastKeyEvent for 2 fast key strokes, a_down b_down a_up b_up, then the a_keyAsChord would not fire 
                {
                    Console.WriteLine("\t/!up: keyDownEvent != e.LastKeyDownEvent");
                    return false;
                }

                if (allUp && keyDownEvent != e.LastKeyDownEvent)
                {
                    Console.WriteLine("\t/!allUp: keyDownEvent != e.LastKeyDownEvent");
                    return false;
                }

                return true;
            }

            return new MetaKeys()
            {
                source.Down(e =>
                {
                    handling     = true;
                    keyDownEvent = e;
                    e.Handled    = true;
                }, predicate, "", KeyStateTree.Map),
                allUp
                    ? source.AllUp(AsyncCall, KeyUpPredicate, "", KeyStateTree.Map)
                    : source.Up(AsyncCall, KeyUpPredicate, "", KeyStateTree.Map)
            };
        }

        /// <summary>
        /// down up happened successively
        /// </summary>
        /// <param name="combination"></param>
        /// <param name="keyCommand"></param>
        /// <param name="canExecute"></param>
        /// <param name="markHandled"></param>
        /// <returns></returns>
        internal static IMetaKey Hit(ICombination combination, KeyCommand keyCommand,
            Predicate<KeyEventArgsExt> canExecute = null, bool markHandled = true)
        {
            var             handling     = false;
            KeyEventArgsExt keyDownEvent = null;
            var token = new MetaKeys()
            {
                combination.Down(e =>
                {
                    if (canExecute == null || canExecute(e))
                    {
                        handling     = true;
                        keyDownEvent = e;

                        if (!markHandled) return;
                        e.Handled = true;
                        return;
                    }

                    Console.WriteLine("\tCanExecute:false");
                    handling = false;
                }),

                combination.Up(e =>
                {
                    if (!handling)
                    {
                        Console.WriteLine("\tHandling:false");
                        return;
                    }

                    handling = false;
                    if (markHandled)
                    {
                        e.Handled = true;
                    }

                    if (keyDownEvent == e.LastKeyDownEvent && (canExecute == null || canExecute(e)))
                    {
                        e.BeginInvoke(() => keyCommand?.Execute(e));
                    }
                    else
                    {
                        Console.WriteLine("\tCondition not meet, Not Execute!");
                    }
                }, null, keyCommand.Description)
            };

            return token;
        }

        public static event KeyPressEventHandler KeyPress
        {
            add => _Hook.KeyPress += value;
            remove => _Hook.KeyPress -= value;
        }

        public static event KeyEventHandler KeyDown
        {
            add => _Hook.KeyDown += value;
            remove => _Hook.KeyDown -= value;
        }

        public static async Task<KeyEventArgsExt> KeyDownAsync(bool handled = false)
        {
            return await TaskExt.FromEvent<KeyEventArgsExt>(e =>
                {
                    if (handled)
                        e.Handled = true;
                })
                .HandlerConversion<KeyEventHandler>(h => new KeyEventHandler(h))
                .Start(h => KeyDown += h, h => KeyDown -= h, CancellationToken.None);
        }

        public static async Task<KeyEventArgsExt> KeyUpAsync(bool handled = false)
        {
            return await TaskExt.FromEvent<KeyEventArgsExt>(e=>{
                    if (handled)
                        e.Handled = true;
                })
                .HandlerConversion<KeyEventHandler>(h => new KeyEventHandler(h))
                .Start(h => KeyUp += h, h => KeyUp -= h, CancellationToken.None);
        }

        public static event KeyEventHandler KeyUp
        {
            add => _Hook.KeyUp += value;
            remove => _Hook.KeyUp -= value;
        }

        public static void HotKey(this string keys, string description, Action action)
        {
            if (keys.Contains(','))
            {
                var sequence = Sequence.FromString(keys).ToList<ICombination>();
                Add(sequence, KeyEvent.Down, new KeyCommand(e => action()) {Description = description});
            }

            var combination = Combination.FromString(keys) as Combination;
            Add(combination, KeyEvent.Down, new KeyCommand(e => action()) {Description = description});
        }

        private static void Async(Action action, DispatcherPriority priority = DispatcherPriority.Send)
        {
            _dispatcher.BeginInvoke(priority, action);
        }

        public static void Type(Keys key, bool IsAsync)
        {
            if (IsAsync)
                Async(() => InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key));
            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);
        }

        public static void Type(Keys[] keys) => InputSimu.Inst.Keyboard.KeyPress(keys.Cast<VirtualKeyCode>().ToArray());

        public static void Type(Keys key) => InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);

        public static void Type(char character) => InputSimu.Inst.Keyboard.Type(character);

        public static void Type(string text) => InputSimu.Inst.Keyboard.Type(text);

        public static void Hook()
        {
            _Hook.Run();
        }
    }
}