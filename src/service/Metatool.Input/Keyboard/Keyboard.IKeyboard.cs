using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input.implementation;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.WindowsInput.Native;
using Microsoft.Extensions.Logging;

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
        public IKeyboardCommandTrigger OnDown(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            return OnEvent(hotkey, KeyEvent.Down, stateTree);
        }

        public IKeyboardCommandTrigger OnUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            return OnEvent(hotkey, KeyEvent.Up, stateTree);
        }

        public IKeyboardCommandTrigger OnHit(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            var trigger = new KeyboardCommandTrigger();
            var token = Hit(hotkey,
                trigger.OnExecute, trigger.OnCanExecute, "", stateTree) as KeyCommandTokens;
            trigger._metaKey = token?.metaKey;
            return trigger;
        }

        public IKeyboardCommandTrigger OnAllUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
        {
            return OnEvent(hotkey, KeyEvent.AllUp, stateTree);
        }

        public IKeyboardCommandTrigger OnEvent(IHotkey hotkey, KeyEvent keyEvent,
            string stateTree = KeyStateTrees.Default)
        {
            if (keyEvent == KeyEvent.Hit)
            {
                return OnHit(hotkey, stateTree);
            }

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

        public IKeyCommand HotString(string source, string target, Predicate<IKeyEventArgs> predicate = null)
        {
            var sequence = Sequence.FromString(source);
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
                        }, k => { InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode) k); });
                    }
                );
            }, predicate, "", KeyStateTrees.HotString);
        }

        public IKeyCommand HardMap(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null)
        {
            return HotKeyMap(source, target, predicate, true);
        }

        public IKeyCommand MapOnDownUp(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null)
        {
            return HotKeyMap(source, target, predicate, false, false);
        }

        IKeyCommand HotKeyMap(IHotkey source, ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate, bool isHardMap, bool isAsync = false)
        {
            void Call(IKeyEventArgs e, Action action)
            {
                if (isAsync)
                {
                    e.BeginInvoke(action);
                    return;
                }

                action();
            }

            var handled     = false;
            var combination = target.ToCombination();
            return new KeyCommandTokens()
            {
                source.Down(e =>
                {
                    handled   = true;
                    e.Handled = true;
                    if (isAsync) e.NoFurtherProcess = true;
                    if (combination.TriggerKey == Keys.LButton)
                    {
                        Call(e, () => InputSimu.Inst.Mouse.LeftDown());
                    }
                    else if (combination.TriggerKey == Keys.RButton)
                    {
                        Call(e, () => InputSimu.Inst.Mouse.RightDown());
                    }
                    else if (combination.TriggerKey == Keys.MButton)
                    {
                        Call(e, () => InputSimu.Inst.Mouse.MiddleDown());
                    }
                    else
                    {
                        Call(e, () => Down(combination));
                    }
                }, predicate, "", isHardMap ? KeyStateTrees.HardMap : KeyStateTrees.Map),
                source.Up(e =>
                {
                    handled   = false;
                    e.Handled = true;
                    if (isAsync) e.NoFurtherProcess = true;
                    if (combination.TriggerKey == Keys.LButton)
                    {
                        Call(e, () => InputSimu.Inst.Mouse.LeftUp());
                    }

                    else if (combination.TriggerKey == Keys.RButton)
                    {
                        Call(e, () => InputSimu.Inst.Mouse.RightUp());
                    }
                    else if (combination.TriggerKey == Keys.MButton)
                    {
                        Call(e, () => InputSimu.Inst.Mouse.MiddleUp());
                    }
                    else
                    {
                        Call(e, () => Up(combination));
                    }
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
                }, "", isHardMap ? KeyStateTrees.HardMap : KeyStateTrees.Map)
            };
        }

        /// <summary>
        ///  Note: A+B -> C become A+C 
        /// </summary>
        public IKeyCommand MapOnHit(IHotkey source, IHotkey target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            return MapOnHitOrAllUp(source, target, predicate, false);
        }

        public IKeyCommand MapOnHitAndAllUp(IHotkey source, IHotkey target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            return MapOnHitOrAllUp(source, target, predicate, true);
        }

        private IKeyCommand MapOnHitOrAllUp(IHotkey source, IHotkey target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = false)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;

            void KeyUpAsyncCall(IKeyEventArgs e)
            {
                e.Handled = true;
                e.BeginInvoke(() => Type(target));
            }

            bool KeyUpPredicate(IKeyEventArgs e)
            {
                if (!handling)
                {
                    Console.WriteLine("\t/!Predicate Handling:false");
                    return false;
                }

                handling = false;
                if (keyDownEvent != e.LastKeyDownEvent)
                {
                    Console.WriteLine(allUp
                        ? "\t/!allUp: keyDownEvent != e.LastKeyDownEvent"
                        : "\t/!up: keyDownEvent != e.LastKeyDownEvent");
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
                }, e =>
                {
                    handling = false;
                    return predicate == null || predicate(e);
                }, "", KeyStateTrees.ChordMap),
                allUp
                    ? source.AllUp(KeyUpAsyncCall, KeyUpPredicate, "", KeyStateTrees.ChordMap)
                    : source.Up(KeyUpAsyncCall, KeyUpPredicate, "", KeyStateTrees.ChordMap)
            };
        }

        public IKeyCommand Map(IHotkey source, IHotkey target, KeyMaps keyMaps,
            Predicate<IKeyEventArgs> predicate = null)
        {
            switch (keyMaps)
            {
                case KeyMaps.HardMap:
                    var t = target as ISequenceUnit;
                    if (t == null)
                        throw new ArgumentException(
                            $"HardMap could only map '{source}' to ISequenceUnit, but currently mapped to {target}");
                    return HardMap(source, t, predicate);
                case KeyMaps.MapOnDownUp:
                    var t1 = target as ISequenceUnit;
                    if (t1 == null)
                        throw new ArgumentException(
                            $"MapOnDownUp could only map '{source}' to ISequenceUnit, but currently mapped to {target}");
                    return MapOnDownUp(source, t1, predicate);
                case KeyMaps.MapOnHit:
                    return MapOnHit(source, target, predicate);
                case KeyMaps.MapOnHitAndAllUp:
                    return MapOnHitAndAllUp(source, target, predicate);

                default:
                    throw new ArgumentOutOfRangeException(nameof(keyMaps), keyMaps, null);
            }
        }

        public IKeyCommand ChordMap(ISequenceUnit source, ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            var           handling     = false;
            IKeyEventArgs keyDownEvent = null;
            ICombination sourCombination = source.ToCombination();
            
            void Handler(Object o, IKeyEventArgs e)
            {
                _hook.KeyDown -= Handler;
                if(sourCombination.IsAnyKey(e.KeyCode)) return;
                e.Handled    =  true;
                Down(new Combination(e.KeyCode, target));
            }

            void KeyUpAsyncCall(IKeyEventArgs e)
            {
                e.Handled = true;
                if (keyDownEvent == e.LastKeyDownEvent_NoneVirtual)
                {
                    e.BeginInvoke(() => Type(source));
                    return;
                }

                e.BeginInvoke(() => Up(target));
            }

            bool KeyUpPredicate(IKeyEventArgs e)
            {
                if (e.IsVirtual) return false;

                _hook.KeyDown -= Handler;
                if (!handling)
                {
                    Console.WriteLine("\t/!Predicate Handling:false");
                    return false;
                }

                handling = false;
                return true;
            }

            return new KeyCommandTokens()
            {
                source.Down(e =>
                {
                    e.Handled = true;
                    handling     = true;
                    e.BeginInvoke(() =>
                        _hook.KeyDown += Handler
                    );
                    keyDownEvent = e;
                }, e =>
                {
                    if (e.IsVirtual || handling) return false;
                    handling = false;
                    return predicate == null || predicate(e);
                }, "", KeyStateTrees.ChordMap),
                source.Up(KeyUpAsyncCall, KeyUpPredicate, "", KeyStateTrees.ChordMap)
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

        readonly IDictionary<string, IHotkey> _aliases    = new Dictionary<string, IHotkey>();
        readonly IDictionary<string, string>  _aliasesRaw = new Dictionary<string, string>();
        public   Dictionary<string, IHotkey>  Aliases => _aliases as Dictionary<string, IHotkey>;

        public bool AddAliases(IDictionary<string, string> aliases)
        {
            var re = true;
            foreach (var alias in aliases)
            {
                if (string.IsNullOrEmpty(alias.Value)) continue;

                var r = Sequence.TryParse(alias.Value, out var key);
                if (!r)
                {
                    re = false;
                    _logger?.LogError($"Could not parse {alias.Value} of alias: {alias.Key}");
                    continue;
                }

                _aliasesRaw.Add(alias.Key, alias.Value);
                _aliases.Add(alias.Key, key);
            }

            return re;
        }

        public string ReplaceAlias(string hotkey, params IDictionary<string, string>[] additionalAliasesDics)
        {
            var aliases = new Dictionary<string, string>(_aliasesRaw);
            if (additionalAliasesDics != null)
                foreach (var aliasesDic in additionalAliasesDics)
                {
                    if (aliasesDic == null) continue;
                    foreach (var alias in aliasesDic)
                    {
                        aliases[alias.Key] = alias.Value;
                    }
                }


            string replaceComma(string str)
            {
                const string comma = "Comma";
                str = Regex.Replace(str, @"^\s*,\s*$", comma);
                str = Regex.Replace(str, @"\+\s*,", $"+ {comma}");
                str = Regex.Replace(str, @",\s*,", $", {comma}");
                return str;
            }

            foreach (var alias in aliases.Reverse())
            {
                hotkey = replaceComma(hotkey);
                hotkey = Regex.Replace(hotkey,  @$"(?:(?<=[\s,+])|^){Regex.Escape(alias.Key)}(?:(?=[\s,+*])|$)", alias
                    .Value);
            }

            return hotkey;
        }

        public bool RegisterKeyMaps(IDictionary<string, string> maps,
            IDictionary<string, string> additionalAliases = null)
        {
            var hasError = false;
            foreach (var map in maps)
            {
                try
                {
                    if (string.IsNullOrEmpty(map.Value))
                    {
                        _logger?.LogInformation($"Key map: {map.Key} is disabled.");
                        continue;
                    }

                    var source = ReplaceAlias(map.Key, additionalAliases);
                    var target = ReplaceAlias(map.Value, additionalAliases);
                    var t      = Combination.Parse(target);
                    var s      = Sequence.Parse(source);
                    s.MapOnDownUp(t);
                }
                catch (Exception e)
                {
                    hasError = true;
                    _logger?.LogError("KeyMappings: " + e.Message);
                }
            }

            return hasError;
        }
    }
}