using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metatool.Input;

[DebuggerDisplay("{ToString()}")]
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

    public override string ToString()
    {
        return $"MetaKey:{MetaKey},Execute:{ExecuteCount},CanExecute:{CanExecuteCount}";
    }
}

public partial class Keyboard : IKeyboard
{

    public IKeyboardCommandTrigger OnDown(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
    {
        return OnEvent(hotkey, KeyEventType.Down, stateTree);
    }

    public IKeyboardCommandTrigger OnUp(IHotkey hotkey, string stateTree = KeyStateTrees.Default)
    {
        return OnEvent(hotkey, KeyEventType.Up, stateTree);
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
        return OnEvent(hotkey, KeyEventType.AllUp, stateTree);
    }

    public IKeyboardCommandTrigger OnEvent(IHotkey hotkey, KeyEventType keyEventType,
        string stateTree = KeyStateTrees.Default)
    {
        if (keyEventType == KeyEventType.Hit)
        {
            return OnHit(hotkey, stateTree);
        }

        var sequence = hotkey switch
        {
            ISequenceUnit sequenceUnit => new List<ICombination>() { sequenceUnit.ToCombination().ToCombination() },
            ISequence seq => seq.ToList(),
            _ => throw new Exception("IHotkey should be ISequence or ISequenceUnit")
        };

        var trigger = new KeyboardCommandTrigger();
        var metaKey = Add(sequence, keyEventType,
            new KeyCommand(trigger.OnExecute) { CanExecute = trigger.OnCanExecute }, stateTree) as MetaKey;
        trigger._metaKey = metaKey;
        return trigger;
    }

    public IKeyCommand HotString(string source, string target, Predicate<IKeyEventArgs> predicate = null)
    {
        var sequence = Sequence.FromHotString(source);
        var send = Enumerable.Repeat(Keys.Back, source.Length).Cast<KeyCodes>();
        return sequence.OnUp(e =>
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
                    },
                k => { InputSimu.Inst.Keyboard.KeyPress((KeyCodes)k); });

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

    IKeyCommand HotKeyMap(IHotkey source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate, bool isHardMap, bool isAsync = false)
    {
        void Call(IKeyEventArgs e, Action action)
        {
            if (isAsync)
            {
                Task.Run(action);
                return;
            }

            action();
        }

        var handled = false;
        var combination = target.ToCombination();

        return new KeyCommandTokens()
        {
            source.OnDown(e =>
            {
                handled   = true;
                e.Handled = true;
                if (isAsync) e.NoFurtherProcess = true;

                if (combination.TriggerKey == KeyCodes.LButton)
                {
                    Call(e, () => InputSimu.Inst.Mouse.LeftDown());
                }
                else if (combination.TriggerKey == KeyCodes.RButton)
                {
                    Call(e, () => InputSimu.Inst.Mouse.RightDown());
                }
                else if (combination.TriggerKey == KeyCodes.MButton)
                {
                    Call(e, () => InputSimu.Inst.Mouse.MiddleDown());
                }
                else
                {
                    Call(e, () => Down(combination));
                }
            }, predicate, "", isHardMap ? KeyStateTrees.HardMap : KeyStateTrees.Map),
            source.OnUp(e =>
            {
                handled   = false;
                e.Handled = true;
                if (isAsync) e.NoFurtherProcess = true;

                if (combination.TriggerKey == KeyCodes.LButton)
                {
                    Call(e, () => InputSimu.Inst.Mouse.LeftUp());
                }

                else if (combination.TriggerKey == KeyCodes.RButton)
                {
                    Call(e, () => InputSimu.Inst.Mouse.RightUp());
                }
                else if (combination.TriggerKey == KeyCodes.MButton)
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
    ///  Note: A+ B -> C become A+C
    /// </summary>
    public IKeyCommand MapOnHit(IHotkey source, IHotkey target,
        Predicate<IKeyEventArgs> predicate = null, string description = "")
    {
        return MapOnHitOrAllUp(source, target, predicate, false, description);
    }

    public IKeyCommand MapOnHitAndAllUp(IHotkey source, IHotkey target,
        Predicate<IKeyEventArgs> predicate = null, string description = "")
    {
        return MapOnHitOrAllUp(source, target, predicate, true, description);
    }

    class NoEventTimer(long eventDuration = 800)
    {
        readonly Stopwatch sw = new();

        public void EventPulse()
        {
            sw.Restart();
        }

        public long NoEventDuration => sw.ElapsedMilliseconds - eventDuration;
    }
    // on key is hold for 5s
    private const int StateResetTime = 5000;

    /// <summary>
    /// hit: the trigger key down and up
    /// allUp: trigger key and the chords are all up
    ///
    /// for A: A down, within delay of RepeatDelay(2000) B down: A is considered as Chord of A+B
    /// </summary>
    private IKeyCommand MapOnHitOrAllUp(IHotkey source, IHotkey target, Predicate<IKeyEventArgs> predicate = null, bool allUp = false, string description = "")
    {
        var delay = _config.CurrentValue?.Services.Input.Keyboard.RepeatDelay ?? 3000;
        // up event is lost. i.e. because of hook take too long for previous event
        var noUpEventTimer = new NoEventTimer();
        // when holding the key
        var holding = false;
        IKeyEventArgs keyDownEvent = null;

        // used when target = source and allUp = false => turn a normal key to the chord, so within the delay it can act as a Chord
        var holdingTimer = new Stopwatch();

        void Reset()
        {
            holding = false;
            keyDownEvent = null;
            holdingTimer.Reset();
            Console.WriteLine("MapOnHitOrAllUp-Reset");
        }

        void KeyUpAsyncCall(IKeyEventArgs e)
        {
            e.Handled = true;
            //e.BeginInvoke(() => Type(target));
            Type(target);

        }

        bool KeyUpPredicate(IKeyEventArgs e)
        {
            if (e.IsVirtual)
            {
                return false;
            }
            if (!holding)
            {
                Console.WriteLine("\t/!MapOnHitOrAllUp-Predicate: Handling==false");
                return false;
            }

            holding = false;
            if (keyDownEvent != e.LastKeyDownEvent)
            {
                Console.WriteLine(allUp
                    ? "\t/!MapOnHitOrAllUp-allUp: keyDownEvent != e.LastKeyDownEvent"
                    : "\t/!MapOnHitOrAllUp-up: keyDownEvent != e.LastKeyDownEvent");
                Reset();
                return false;
            }
            Reset();
            return true;
        }

        KeyDown += (_, e) =>
        {
            if (keyDownEvent != null && e.KeyCode != keyDownEvent.KeyCode)
            {
                Console.WriteLine($"MapOnHitOrAllUp:{e.KeyCode} canceled because other key is down!");
                Reset();
            }
        };

        return new KeyCommandTokens()
        {
            source.OnDown(
                    e =>
                {
                    e.Handled    = true;

                    keyDownEvent = e;
                    if (holding)
                        return; // repeated long press key, within duration, when holding the key as the chord

                    holding = true;
                    holdingTimer.Restart();
                },
                    e =>
                {
                    var noEventDuration = noUpEventTimer.NoEventDuration;
                    if (noEventDuration > StateResetTime) Reset();
                    noUpEventTimer.EventPulse();

                    if ((!holding || holdingTimer.ElapsedMilliseconds <= delay) && (predicate == null || predicate(e)))
                        return true;

                    if (holdingTimer.IsRunning) holdingTimer.Stop();
                    Console.WriteLine($"!holding:{holding} && holdingTimer.ElapsedMilliseconds:{holdingTimer.ElapsedMilliseconds}<=delay:{delay}");

                    return false; // disable map
                },
                description, KeyStateTrees.ChordMap
            ),
            allUp?
            source.OnAllUp(KeyUpAsyncCall, KeyUpPredicate, description, KeyStateTrees.ChordMap):
            source.OnUp(KeyUpAsyncCall, KeyUpPredicate, description, KeyStateTrees.ChordMap)
        };
    }

    public IKeyCommand Map(IHotkey source, IHotkey target, KeyMaps keyMaps, Predicate<IKeyEventArgs> predicate = null)
    {
        switch (keyMaps)
        {
            case KeyMaps.HardMap:
                if (target is not ISequenceUnit t)
                    throw new ArgumentException(
                        $"HardMap could only map '{source}' to ISequenceUnit, but currently mapped to {target}");
                return HardMap(source, t, predicate);
            case KeyMaps.MapOnDownUp:
                if (target is not ISequenceUnit t1)
                    throw new ArgumentException(
                        $"MapOnDownUp could only map '{source}' to ISequenceUnit, but currently mapped to {target}");
                return MapOnDownUp(source, t1, predicate);
            case KeyMaps.MapOnHit:
                return MapOnHit(source, target, predicate);
            case KeyMaps.MapOnHitAndAllUp:
                return MapOnHitAndAllUp(source, target, predicate);
            case KeyMaps.ChordMap:
                {
                    if (source is not ISequenceUnit src)
                        throw new ArgumentException(
                            $"ChordMap could only use ISequenceUnit as it's source,  but currently the source is {source}");
                    if (target is not ISequenceUnit tgt)
                        throw new ArgumentException(
                            $"ChordMap could only use ISequenceUnit as it's target,  but currently the source is {target}");

                    return ChordMap(src, tgt, predicate);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(keyMaps), keyMaps, null);
        }
    }

    public IKeyCommand ChordMap(ISequenceUnit source, ISequenceUnit target, Predicate<IKeyEventArgs> predicate = null)
    {
        var sourCombination = source.ToCombination();
        var delay = _config?.CurrentValue?.Services?.Input?.Keyboard?.RepeatDelay ?? 3000;
        // state
        var handling = false;
        IKeyEventArgs keyDownEvent = null;
        var stopwatch = new Stopwatch();
        var targetDown = false;
        var noEventTimer = new NoEventTimer();

        void Reset()
        {
            handling = false;
            keyDownEvent = null;
            stopwatch.Reset();
            targetDown = false;
        }
        void Handler(object o, IKeyEventArgs e)
        {
            _hook.KeyDown -= Handler;
            if (sourCombination.IsAnyKey(e.KeyData)) return; // repeated long press key, > duration

            e.Handled = true;
            Down(new Combination(e.KeyData, target));
            targetDown = true;
        }

        return new KeyCommandTokens()
        {
            source.OnDown(e =>
            {
                e.Handled    = true;
                keyDownEvent = e;
                if (handling) return; // repeated long press key, within duration

                handling = true;
                stopwatch.Restart();
                _hook.KeyDown += Handler; // not tested, removed async register.
                //e.BeginInvoke(()=>_hook.KeyDown += Handler); //have to be async to handle next key down event, otherwise can't capture it.
            },
                e =>
            {
                var noEventDuration = noEventTimer.NoEventDuration;
                if (noEventDuration > StateResetTime) Reset();
                noEventTimer.EventPulse();

                var duration = stopwatch.ElapsedMilliseconds;
                if (!e.IsVirtual && (!handling || duration <= delay) && (predicate == null || predicate(e)))
                    return true;
                if (stopwatch.IsRunning) stopwatch.Stop();
                return false;
            }, "", KeyStateTrees.ChordMap),

            source.OnUp(e =>
            {
                if (keyDownEvent == e.LastKeyDownEvent_NoneVirtual)
                {
                    e.Handled = true;
                    if (targetDown) Up(target); // fix: for logic wrong, if typing fast, the up event of source is not fired but another key down happens.
                    targetDown = false;
                    var downKeysLast = keyDownEvent.KeyboardState.DownKeys;
                    var downKeys = e.KeyboardState.DownKeys;
                    downKeysLast = downKeysLast.Where(k => !downKeys.Contains(k));
                    var sourceComb = source.ToCombination();
                    var combination = new Combination(sourceComb.TriggerKey, downKeysLast.Concat(sourceComb.Chord));
                    Type(combination);
                    return;
                }

                if (!targetDown)
                    return;

                e.Handled = true;
                Up(target);
                targetDown = false;
            }, e =>
            {
                if (e.IsVirtual) return false;

                _hook.KeyDown -= Handler;
                if (!handling)
                {
                    Console.WriteLine("\t/!Predicate Handling:false");
                    return false;
                }

                handling = false;
                if (stopwatch.IsRunning) stopwatch.Reset();
                return true;
            }, "", KeyStateTrees.ChordMap)
        };
    }

    public async Task<IKeyEventArgs> KeyDownAsync(bool handled = false, int timeout = Timeout.Infinite, ISequenceUnit hotkey = null, CancellationToken token = default)
    {
        return await TaskExt.FromEvent<IKeyEventArgs>(timeout, e =>
            {
                if (handled)
                    e.Handled = true;
            }, e => hotkey == null || e.IsActive(hotkey))
            .HandlerConversion(h => new MouseKeyHook.KeyEventHandler(h))
            .Start(h => KeyDown += h, h => KeyDown -= h, token == default ? CancellationToken.None : token);
    }

    public async Task<IKeyPressEventArgs> KeyPressAsync(bool handled = false, int timeout = Timeout.Infinite, ISequenceUnit hotkey = null, CancellationToken token = default)
    {
        return await TaskExt.FromEvent<IKeyPressEventArgs>(timeout, e =>
            {
                if (handled)
                    e.Handled = true;
            }, e => hotkey == null || e.IsActive(hotkey))
            .HandlerConversion(h => new MouseKeyHook.KeyPressEventHandler(h))
            .Start(h => KeyPress += h, h => KeyPress -= h, token == default ? CancellationToken.None : token);
    }

    public async Task<IKeyEventArgs> KeyUpAsync(bool handled = false, int timeout = Timeout.Infinite, ISequenceUnit hotkey = null, CancellationToken token = default)
    {
        return await TaskExt.FromEvent<IKeyEventArgs>(timeout, e =>
            {
                if (handled)
                    e.Handled = true;
            }, e => hotkey == null || e.IsActive(hotkey))
            .HandlerConversion(h => new MouseKeyHook.KeyEventHandler(h))
            .Start(h => KeyUp += h, h => KeyUp -= h, token == default ? CancellationToken.None : token);
    }

    readonly IDictionary<string, IHotkey> _aliases = new Dictionary<string, IHotkey>();
    readonly IDictionary<string, string> _aliasesRaw = new Dictionary<string, string>();
    public Dictionary<string, IHotkey> Aliases => _aliases as Dictionary<string, IHotkey>;

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

    public string ReplaceAlias(string hotkey, params IDictionary<string, string>[] additionalTempAliasesDics)
    {
        var aliasesRaw = new Dictionary<string, string>(_aliasesRaw);
        if (additionalTempAliasesDics != null)
            foreach (var aliasesDic in additionalTempAliasesDics)
            {
                if (aliasesDic == null) continue;
                foreach (var alias in aliasesDic)
                {
                    aliasesRaw[alias.Key] = alias.Value;
                }
            }


        static string ReplaceComma(string str)
        {
            const string comma = "Comma";
            str = Regex.Replace(str, @"^\s*,\s*$", comma);
            str = Regex.Replace(str, @"\+\s*,", $"+ {comma}");
            str = Regex.Replace(str, @",\s*,", $", {comma}");
            return str;
        }

        foreach (var alias in aliasesRaw.Reverse())
        {
            hotkey = ReplaceComma(hotkey);
            hotkey = Regex.Replace(hotkey, @$"(?:(?<=[\s,+])|^){Regex.Escape(alias.Key)}(?:(?=[\s,+*])|$)", alias.Value);
        }

        return hotkey;
    }

    public bool RegisterKeyMaps(IDictionary<string, KeyMapDef> maps,
        IDictionary<string, string> additionalAliases = null)
    {
        var hasError = false;
        foreach (var map in maps)
        {
            try
            {
                if (string.IsNullOrEmpty(map.Value.Target))
                {
                    _logger?.LogInformation($"Key map: {map.Key} is disabled.");
                    continue;
                }

                var source = ReplaceAlias(map.Key, additionalAliases);
                var target = ReplaceAlias(map.Value.Target, additionalAliases);
                var t = Hotkey.Parse(target);
                var s = Hotkey.Parse(source);
                Map(s, t, map.Value.Type);
            }
            catch (Exception e)
            {
                hasError = true;
                _logger?.LogError("KeyMappings: " + e.Message);
            }
        }

        return hasError;
    }

    public bool IsDown(IKey key)
    {
        var state = KeyboardState.Current();
        return state.IsDown((Key)key);
    }
    public bool IsUp(IKey key)
    {
        var state = KeyboardState.Current();
        return state.IsUp((Key)key);
    }
    public bool IsToggled(IKey key)
    {
        var state = KeyboardState.Current();
        return state.IsToggled((Key)key);
    }
    public bool Disable
    {
        get => _hook.Disable;
        set => _hook.Disable = value;
    }
    public bool DisableDownEvent
    {
        get => _hook.DisableDownEvent;
        set => _hook.DisableDownEvent = value;
    }
    public bool DisableUpEvent
    {
        get => _hook.DisableUpEvent;
        set => _hook.DisableUpEvent = value;
    }
    public bool DisablePressEvent
    {
        get => _hook.DisablePressEvent;
        set => _hook.DisablePressEvent = value;
    }

    public void DisableChord(ISequenceUnit chord)
    {
        var comb = chord.ToCombination() as Combination;
        Debug.Assert(comb != null, nameof(comb) + " != null");
        var c = comb.ToChord();
        _hook.DisableChord(c);
    }

    public void EnableChord(ISequenceUnit chord)
    {
        var comb = chord.ToCombination() as Combination;
        Debug.Assert(comb != null, nameof(comb) + " != null");
        var c = comb.ToChord();
        _hook.EnableChord(c);
    }

    public IKeyboardState State => KeyboardState.Current();
}