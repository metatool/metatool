using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.WindowsInput.Native;

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
        public IKeyboardCommandTrigger Down(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            return Event(hotkey, KeyEvent.Down, stateTree);
        }

        public IKeyboardCommandTrigger Up(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            return Event(hotkey, KeyEvent.Up, stateTree);
        }

        public IKeyboardCommandTrigger AllUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            return Event(hotkey, KeyEvent.AllUp, stateTree);
        }

        public IKeyboardCommandTrigger Hit(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            var trigger     = new KeyboardCommandTrigger();
            var token = Hit(hotkey,
                trigger.OnExecute, trigger.OnCanExecute, "", stateTree) as KeyCommandTokens;
            trigger._metaKey = token?.metaKey;
            return trigger;
        }

        private IKeyboardCommandTrigger Event(IHotkey hotkey, KeyEvent keyEvent,
            string stateTree = KeyStateTrees.Default)
        {
            var sequence = hotkey switch
            {
                ISequenceUnit sequenceUnit => new List<ICombination>() {sequenceUnit.ToCombination().ToCombination()},
                ISequence seq => seq.ToList(),
                _ => throw new Exception("IHotkey should be ISequence or ISequenceUnit")
            };
            var trigger = new KeyboardCommandTrigger();
            var metaKey = Add(sequence, keyEvent,
                new KeyCommand(trigger.OnExecute) {CanExecute = trigger.OnCanExecute}, stateTree) as MetaKey;
            trigger._metaKey = metaKey;
            return trigger;
        }

        public IKeyCommand HardMap(IHotkey source, ICombination target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            var handled = false;
            return new KeyCommandTokens()
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

        public IKeyCommand Map(string source, string target, Predicate<IKeyEventArgs> predicate = null)
        {
            var sequence = Sequence.FromString(string.Join(",", source.ToUpper().ToCharArray()));
            var send     = Enumerable.Repeat(Keys.Back, source.Length).Cast<VirtualKeyCode>();
            return sequence.Up(e =>
            {
                e.BeginInvoke(() =>
                    {
                        var notify = Services.Get<INotify>();
                        notify.ShowSelectionAction(new[]
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


        public IKeyCommand Map(IHotkey source, ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null, int repeat = 1)
        {
            var handled = false;
            var combination = target.ToCombination();
            return new KeyCommandTokens()
            {
                source.Down(e =>
                {
                    handled   = true;
                    e.Handled = true;

                    if (combination.TriggerKey == Keys.LButton)
                    {
                        Async(Repeat(repeat, () => InputSimu.Inst.Mouse.LeftDown()));
                    }
                    else if (combination.TriggerKey == Keys.RButton)
                    {
                        Async(Repeat(repeat, () => InputSimu.Inst.Mouse.RightDown()));
                    }
                    else
                    {
                        Async(Repeat(repeat, () => InputSimu.Inst.Keyboard.ModifiedKeyDown(
                            combination.Chord.Cast<VirtualKeyCode>(),
                            (VirtualKeyCode) (Keys) combination.TriggerKey)
                        ));
                    }
                }, predicate, "", KeyStateTrees.Map),
                source.Up(e =>
                {
                    if (!handled) return;
                    handled   = false;
                    e.Handled = true;
                    if (combination.TriggerKey == Keys.LButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.LeftUp());
                        return;
                    }

                    if (combination.TriggerKey == Keys.RButton)
                    {
                        Async(() => InputSimu.Inst.Mouse.RightUp());
                        return;
                    }

                    InputSimu.Inst.Keyboard.ModifiedKeyUp(combination.Chord.Cast<VirtualKeyCode>(),
                        (VirtualKeyCode) (Keys) combination.TriggerKey);
                }, predicate, "", KeyStateTrees.Map)
            };
        }

        public IKeyCommand MapOnHit(IHotkey source, ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;
            var combination = target.ToCombination();

            void AsyncCall(IKeyEventArgs e)
            {
                e.Handled = true;
                e.BeginInvoke(() => InputSimu.Inst.Keyboard.ModifiedKeyStroke(
                    combination.Chord.Select(k => (VirtualKeyCode) (Keys) k),
                    (VirtualKeyCode) (Keys) combination.TriggerKey));
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

            return new KeyCommandTokens()
            {
                source.Down(e =>
                {
                    handling     = true;
                    keyDownEvent = e;
                    e.Handled    = true;
                }, predicate, "", KeyStateTrees.ChordMap),
                allUp
                    ? source.AllUp(AsyncCall, KeyUpPredicate, "", KeyStateTrees.ChordMap)
                    : source.Up(AsyncCall, KeyUpPredicate, "", KeyStateTrees.ChordMap)
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
                .HandlerConversion(h => new MouseKeyHook.KeyEventHandler(h))
                .Start(h => KeyUp += h, h => KeyUp -= h, token == default ? CancellationToken.None : token);
        }
    }
}