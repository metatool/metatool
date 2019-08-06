using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using WindowsInput.Native;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;
using System.Windows.Threading;
using OneOf;
namespace Metaseed.Input
{
using Hotkey=OneOf<ISequenceUnit,ISequence>;
    public static class Keyboard
    {
        public static   IKeyState    Root        = null;
        static readonly KeyboardHook _Hook       = new KeyboardHook();
        static          Dispatcher   _dispatcher = Dispatcher.CurrentDispatcher;

        internal static IMetaKey Add(ICombination combination, KeyEvent keyEvent, KeyCommand command,
            KeyStateMachine stateMachine = null)
        {
            return Add(new List<ICombination>() {combination}, keyEvent, command, stateMachine);
        }

        internal static IMetaKey Add(IList<ICombination> combinations, KeyEvent keyEvent, KeyCommand command,
            KeyStateMachine stateMachine = null)
        {
            return _Hook.Add(combinations, new KeyEventAction(keyEvent, command), stateMachine);
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
        internal static IMetaKey Map(ICombination source, ICombination target,
            Predicate<KeyEventArgsExt> predicate = null, int repeat = 1)
        {
            var handled = false;
            return new MetaKeys()
            {
                source.Down(e =>
                {
                    if (predicate == null || predicate(e))
                    {
                        handled   = true;
                        e.Handled = true;

                        if (target.TriggerKey == Keys.LButton)
                        {
                            Async(Repeat(repeat, () => InputSimu.Inst.Mouse.LeftButtonDown()));
                        }
                        else if (target.TriggerKey == Keys.RButton)
                        {
                            Async(Repeat(repeat, () => InputSimu.Inst.Mouse.RightButtonDown()));
                        }
                        else
                        {
                            Async(Repeat(repeat,() => InputSimu.Inst.Keyboard.ModifiedKeyDown(target.Chord.Cast<VirtualKeyCode>(),
                                    (VirtualKeyCode)(Keys) target.TriggerKey)
                            ));
                        }

                        return;
                    }

                    handled = false;
                }),
                source.Up(e =>
                {
                    if (!handled) return;
                    handled = false;
                    if (predicate != null && !predicate(e)) return;
                    e.Handled = true;
                    if (target.TriggerKey == Keys.LButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.LeftButtonUp());
                        return;
                    }
                    else if (target.TriggerKey == Keys.RButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.RightButtonUp());
                        return;
                    }

                    InputSimu.Inst.Keyboard.ModifiedKeyUp(target.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys)target.TriggerKey);
                })
            };
        }

        internal static IMetaKey MapOnHit(ICombination source, ICombination target,
            Predicate<KeyEventArgsExt> predicate = null, bool allUp = true)
        {
            var             handling     = false;
            KeyEventArgsExt keyDownEvent = null;

            void AsyncCall(KeyEventArgsExt e)
            {
                e.BeginInvoke(() => InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                    target.Chord.Select(k=>(VirtualKeyCode)(Keys)k),
                    (VirtualKeyCode) (Keys)target.TriggerKey));
            }

            IMetaKey AllKeyUp()
            {
                void KeyUpHandler(object s, KeyEventArgsExt e)
                {
                    if (keyDownEvent != e.LastKeyDownEvent ||
                        !source.IsAnyKey(e.KeyCode)) return;

                    if (handling && e.KeyCode == source.TriggerKey)
                    {
                        handling = false;
                    }

                    var up = e.KeyboardState.IsUp(source.TriggerKey) && e.KeyboardState.AreAllUp(source.Chord);
                    if (!up) return;
                    AsyncCall(e);
                }

                _Hook.KeyUp += KeyUpHandler;
                return new Removable(() => _Hook.KeyUp -= KeyUpHandler);
            }

            return new MetaKeys()
            {
                source.Down(e =>
                {
                    if (predicate == null || predicate(e))
                    {
                        handling     = true;
                        keyDownEvent = e;
                        e.Handled = true;
                        return;
                    }

                    handling = false;
                }),
                allUp
                    ? AllKeyUp()
                    : source.Up(e =>
                    {
                        if (!handling) return;
                        handling  = false;
                        e.Handled = true;

                        if (keyDownEvent != e.LastKeyDownEvent) return;

                        AsyncCall(e);
                    })
            };
        }

        /// <summary>
        /// down up happened successively
        /// </summary>
        /// <param name="combination"></param>
        /// <param name="keyCommand"></param>
        /// <param name="predicate"></param>
        /// <param name="markHandled"></param>
        /// <returns></returns>
        internal static IMetaKey Hit(ICombination combination, KeyCommand keyCommand,
            Predicate<KeyEventArgsExt> predicate = null, bool markHandled = true)
        {
            var             handling     = false;
            KeyEventArgsExt keyDownEvent = null;
            var token = new MetaKeys()
            {
                combination.Down(e =>
                {
                    if (predicate == null || predicate(e))
                    {
                        handling = true;
                        keyDownEvent = e;

                        if (!markHandled) return;
                        e.Handled = true;
                        return;
                    }

                    handling = false;
                }),

                combination.Up(e =>
                {
                    if (!handling) return;

                    handling = false;
                    if (markHandled)
                    {
                        e.Handled = true;
                    }

                    if (keyDownEvent == e.LastKeyDownEvent && (predicate == null || predicate(e)))
                    {
                        e.BeginInvoke(() => keyCommand?.Execute(e));
                    }
                }, keyCommand.Description)
            };

            return token;
        }

        public static event KeyPressEventHandler KeyPress
        {
            add => _Hook.KeyPress += value;
            remove => _Hook.KeyPress -= value;
        }

        public static void HotKey(this string keys, string description, Action action)
        {
            if (keys.Contains(','))
            {
                var sequence = Sequence.FromString(keys).ToList<ICombination>();
                Add(sequence, KeyEvent.Down, new KeyCommand(  e => action()){Description = description});
            }

            var combination = Combination.FromString(keys) as Combination;
            Add(combination, KeyEvent.Down, new KeyCommand(e => action()){Description = description});
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