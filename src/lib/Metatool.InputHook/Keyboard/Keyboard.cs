using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Metatool.Core;
using Metatool.Input.MouseKeyHook;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MetaKeyboard;
using Metatool.WindowsInput.Native;
using Microsoft.Extensions.Logging;
using OneOf;
using KeyEventHandler = Metatool.Input.MouseKeyHook.KeyEventHandler;

namespace Metatool.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public class Keyboard : IKeyboard
    {
        private readonly ILogger<Keyboard> _logger;
        private static Keyboard _default;
        public static Keyboard Default => _default ??= (ServiceLocator.Current.GetService(typeof(IKeyboard)) as Keyboard);

        public Keyboard(ILogger<Keyboard> logger)
        {
            _logger = logger;
        }

        public IKeyPath Root = null;

        readonly KeyboardHook _hook =
            new KeyboardHook(ServiceLocator.Current.GetService(typeof(ILogger<KeyboardHook>)) as ILogger<KeyboardHook>);

        readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        internal IMetaKey Add(ICombination combination, KeyEvent keyEvent, KeyCommand command,
            KeyStateTree stateTree = null)
        {
            return Add(new List<ICombination> {combination}, keyEvent, command, stateTree);
        }

        internal IMetaKey Add(IList<ICombination> combinations, KeyEvent keyEvent, KeyCommand command,
            KeyStateTree stateTree = null)
        {
            return _hook.Add(combinations, new KeyEventCommand(keyEvent, command), stateTree);
        }

        public void ShowTip()
        {
            _hook.ShowTip();
        }

        public void Hit(Keys key, IEnumerable<Keys> modifierKeys = null, bool isAsync = false)
        {
            if (isAsync)
            {
                Async(() => Hit(key, modifierKeys));
                return;
            }

            if (modifierKeys == null) InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);
            InputSimu.Inst.Keyboard.ModifiedKeyStroke(modifierKeys.Cast<VirtualKeyCode>(),
                (VirtualKeyCode) key);
        }

        private Action Repeat(int repeat, Action action)
        {
            return () =>
            {
                while (repeat-- > 0) action();
            };
        }

        internal IMetaKey HardMap(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            var handled = false;
            return new MetaKeys
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

        internal IMetaKey Map(string source, string target, Predicate<IKeyEventArgs> predicate = null)
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


        internal IMetaKey Map(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, int repeat = 1)
        {
            var handled = false;
            return new MetaKeys
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

                    if (target.TriggerKey == Keys.RButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.RightUp());
                        return;
                    }

                    InputSimu.Inst.Keyboard.ModifiedKeyUp(target.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys) target.TriggerKey);
                }, predicate, "", KeyStateTree.Map)
            };
        }

        internal IMetaKey MapOnHit(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;

            void AsyncCall(IKeyEventArgs e)
            {
                e.Handled = true;
                e.BeginInvoke(() => InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                    target.Chord.Select(k => (VirtualKeyCode) (Keys) k),
                    (VirtualKeyCode) (Keys) target.TriggerKey));
            }

            // if not: A+B -> C become A+C
            bool KeyUpPredicate(IKeyEventArgs e)
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

            return new MetaKeys
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
        internal IMetaKey Hit(ICombination combination, KeyCommand keyCommand,
            Predicate<IKeyEventArgs> canExecute = null, bool markHandled = true)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;
            var token = new MetaKeys
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

        public event KeyPressEventHandler KeyPress
        {
            add => _hook.KeyPress += value;
            remove => _hook.KeyPress -= value;
        }

        public event KeyEventHandler KeyDown
        {
            add => _hook.KeyDown += value;
            remove => _hook.KeyDown -= value;
        }

        public async Task<IKeyEventArgs> KeyDownAsync(bool handled = false)
        {
            return await TaskExt.FromEvent<IKeyEventArgs>(e =>
                {
                    if (handled)
                        e.Handled = true;
                })
                .HandlerConversion(h => new KeyEventHandler(h))
                .Start(h => KeyDown += h, h => KeyDown -= h, CancellationToken.None);
        }

        public async Task<IKeyEventArgs> KeyUpAsync(bool handled = false)
        {
            return await TaskExt.FromEvent<IKeyEventArgs>(e =>
                {
                    if (handled)
                        e.Handled = true;
                })
                .HandlerConversion(h => new KeyEventHandler(h))
                .Start(h => KeyUp += h, h => KeyUp -= h, CancellationToken.None);
        }

        public event KeyEventHandler KeyUp
        {
            add => _hook.KeyUp += value;
            remove => _hook.KeyUp -= value;
        }

        internal void HotKey(string keys, string description, Action action)
        {
            if (keys.Contains(','))
            {
                var sequence = Sequence.FromString(keys).ToList();
                Add(sequence, KeyEvent.Down, new KeyCommand(e => action()) {Description = description});
            }

            var combination = Combination.FromString(keys) as Combination;
            Add(combination, KeyEvent.Down, new KeyCommand(e => action()) {Description = description});
        }

        private void Async(Action action, DispatcherPriority priority = DispatcherPriority.Send)
        {
            _dispatcher.BeginInvoke(priority, action);
        }

        public void Type(Keys key, bool IsAsync)
        {
            if (IsAsync)
                Async(() => InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key));
            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);
        }

        public void Type(Keys[] keys) => InputSimu.Inst.Keyboard.KeyPress(keys.Cast<VirtualKeyCode>().ToArray());

        public void Type(Keys key) => InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) key);

        public void Type(char character) => InputSimu.Inst.Keyboard.Type(character);

        public void Type(string text) => InputSimu.Inst.Keyboard.Type(text);

        public void Hook()
        {
            _hook.Run();
        }
    }
}