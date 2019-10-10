using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MetaKeyboard;
using Metatool.WindowsInput.Native;
using KeyEventHandler = Metatool.Input.MouseKeyHook.KeyEventHandler;

namespace Metatool.Input
{
    public class KeyboardCommandTrigger : CommandTrigger<IKeyEventArgs>, IKeyboardCommandTrigger
    {
        internal IMetaKey _metaKey;

        public IMetaKey MetaKey => _metaKey;

        public override void OnAdd(ICommand<IKeyEventArgs> command)
        {
            _metaKey?.ChangeDescription(command.Description);
            base.OnAdd(command);
        }

        public override void OnRemove(ICommand<IKeyEventArgs> command)
        {
            _metaKey?.Remove();
            base.OnRemove(command);
        }
    }

    public partial class Keyboard
    {
        public IKeyboardCommandTrigger Down(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Event(sequenceUnit, KeyEvent.Down);
        }

        public IKeyboardCommandTrigger Up(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Event(sequenceUnit, KeyEvent.Up);
        }

        public IKeyboardCommandTrigger AllUp(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Event(sequenceUnit, KeyEvent.AllUp);
        }

        public IKeyboardCommandTrigger Hit(ISequenceUnit sequenceUnit, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger     = new KeyboardCommandTrigger();
            var token = Hit(combination, 
                trigger.OnExecute, trigger.OnCanExecute,"", stateTree) as KeyboardCommandTokens;
            trigger._metaKey = token?.metaKey;
            return trigger;
        }

        public IKeyboardCommandTrigger Down(ISequence sequence, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Event(sequence, KeyEvent.Down);
        }

        public IKeyboardCommandTrigger Up(ISequence sequence, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Event(sequence, KeyEvent.Up);
        }

        public IKeyboardCommandTrigger AllUp(ISequence sequence, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            return Event(sequence, KeyEvent.AllUp);
        }

        private IKeyboardCommandTrigger Event(ISequence sequence, KeyEvent keyEvent, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            var seq     = sequence as Sequence;
            var trigger = new KeyboardCommandTrigger();
            var metaKey = Add(seq.ToList(), keyEvent,
                new KeyCommand(trigger.OnExecute) {CanExecute = trigger.OnCanExecute}, stateTree) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }

        private IKeyboardCommandTrigger Event(ISequenceUnit sequenceUnit, KeyEvent keyEvent, KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger     = new KeyboardCommandTrigger();
            var metaKey = Add(combination, keyEvent,
                new KeyCommand(trigger.OnExecute) {CanExecute = trigger.OnCanExecute}, stateTree) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }

        public IKeyboardCommandToken HardMap(ICombination source, ICombination target,
          Predicate<IKeyEventArgs> predicate = null)
        {
            var handled = false;
            return new KeyboardCommandTokens()
            {
                source.Down(e =>
                {
                    handled            = true;
                    e.Handled          = true;
                    e.NoFurtherProcess = true;

                    InputSimu.Inst.Keyboard.ModifiedKeyDown(
                        target.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys) target.TriggerKey);
                }, predicate, "", KeyStateTrees.HardMap),
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
                }, "", KeyStateTrees.HardMap)
            };
        }

        public IKeyboardCommandToken Map(string source, string target, Predicate<IKeyEventArgs> predicate = null)
        {
            var sequence = Sequence.FromString(string.Join(",", source.ToUpper().ToCharArray()));
            var send = Enumerable.Repeat(Keys.Back, source.Length).Cast<VirtualKeyCode>();
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
            }, predicate, "", KeyStateTrees.HotString);
        }


        public IKeyboardCommandToken Map(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, int repeat = 1)
        {
            var handled = false;
            return new KeyboardCommandTokens()
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
                }, predicate, "", KeyStateTrees.Map),
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
                }, predicate, "", KeyStateTrees.Map)
            };
        }

        public IKeyboardCommandToken MapOnHit(ICombination source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true)
        {
            var handling = false;
            IKeyEventArgs keyDownEvent = null;

            void AsyncCall(IKeyEventArgs e)
            {
                e.Handled = true;
                e.BeginInvoke(() => InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                    target.Chord.Select(k => (VirtualKeyCode)(Keys)k),
                    (VirtualKeyCode)(Keys)target.TriggerKey));
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

            return new KeyboardCommandTokens()
            {
                source.Down(e =>
                {
                    handling     = true;
                    keyDownEvent = e;
                    e.Handled    = true;
                }, predicate, "", KeyStateTrees.Map),
                allUp
                    ? source.AllUp(AsyncCall, KeyUpPredicate, "", KeyStateTrees.Map)
                    : source.Up(AsyncCall, KeyUpPredicate, "", KeyStateTrees.Map)
            };
        }
        public async Task<IKeyEventArgs> KeyDownAsync(bool handled = false, CancellationToken token = default)
        {
            return await TaskExt.FromEvent<IKeyEventArgs>(e =>
                {
                    if (handled)
                        e.Handled = true;
                })
                .HandlerConversion(h => new MouseKeyHook.KeyEventHandler(h))
                .Start(h => KeyDown += h, h => KeyDown -= h, token == default ? CancellationToken.None : token);
        }

        public async Task<IKeyEventArgs> KeyUpAsync(bool handled = false, CancellationToken token = default)
        {
            return await TaskExt.FromEvent<IKeyEventArgs>(e =>
                {
                    if (handled)
                        e.Handled = true;
                })
                .HandlerConversion(h => new KeyEventHandler(h))
                .Start(h => KeyUp += h, h => KeyUp -= h, token == default ? CancellationToken.None : token);
        }

    }
}