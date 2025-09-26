using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;

namespace Metatool.Input;

public enum TreeType
{
    Default,

    // one for every down, up or allUp KeyEvent
    SingleEventCommand
};

[DebuggerDisplay("${Name}")]
public partial class KeyStateTree
{
    IKeyTipNotifier _notify;

    public KeyStateTree(string name, IKeyTipNotifier notify)
    {
        Name = name;
        _treeWalker = new TrieWalker<ICombination, KeyEventCommand>(_trie);
        _lastKeyDownNodeForAllUp = null;
        _notify = notify;
    }

    public TreeType TreeType = TreeType.Default;

    private readonly Trie<ICombination, KeyEventCommand> _trie = new();
    private readonly TrieWalker<ICombination, KeyEventCommand> _treeWalker;
    public string Name;

    internal KeyProcessState ProcessState;

    internal TrieNode<ICombination, KeyEventCommand> CurrentNode => _treeWalker.CurrentNode;

    internal bool IsOnRoot => _treeWalker.IsOnRoot;

    public void Reset()
    {
        var lastDownHit = "";

        if (_lastKeyDownNodeForAllUp != null)
            lastDownHit = $"last↓@ {_lastKeyDownNodeForAllUp}";
        _lastKeyDownNodeForAllUp = null;

        Console.WriteLine($"${Name}{lastDownHit}");

        Task.Run(() => _notify?.CloseKeysTip(Name)); // use task here, because the slow startup
        _treeWalker.GoToRoot();
    }

    internal bool Contains(IHotkey hotKey)
    {
        var values = hotKey switch
        {
            ISequenceUnit k => _trie.Get((List<ICombination>)[.. k.ToCombination()]),
            ISequence s => _trie.Get(s.ToList()),
            _ => throw new Exception("not supported!")
        };

        return values.Any();
    }

    internal void MarkDoneIfYield()
    {
        if (ProcessState == KeyProcessState.Yield)
        {
            ProcessState = KeyProcessState.Done;
            Console.WriteLine($"${Name}@Yield->@Done");
        }
    }

    public IEnumerable<(string key, IEnumerable<string> descriptions)> Tips(bool ifRootThenEmpty = false)
    {
        if (ifRootThenEmpty && _treeWalker.CurrentNode == _treeWalker.Root)
        {
            return [];
        }

        return _treeWalker.CurrentNode.Tip;
    }

    public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command)
    {
        if (TreeType == TreeType.SingleEventCommand)
        {
            var commands = _trie.Get(combinations);
            if (commands.Count() != 0)
            {
                _trie.Remove(combinations, c => c.KeyEventType == command.KeyEventType);
            }
        }

        _trie.Add(combinations, command);
        return new MetaKey(_trie, combinations, command);
    }

    public IMetaKey Add(ICombination combination, KeyEventCommand command)
    {
        return Add((List<ICombination>)[combination], command);
    }

    private TrieNode<ICombination, KeyEventCommand>? _lastKeyDownNodeForAllUp;

    private readonly HashSet<Chord> _disabledChords = [];

    internal void DisableChord(Chord chord)
    {
        _disabledChords.Add(chord);
    }
    internal void EnableChord(Chord chord)
    {
        _disabledChords.Remove(chord);
    }
    internal SelectionResult TrySelect(KeyEventType eventTypeType, IKeyEventArgs args)
    {
        // to handle A+B+C(B is down in Chord)
        var downInChord = false;

        var type = eventTypeType;
        var candidateNode = _treeWalker.GetChildOrNull((ICombination acc, ICombination combination) =>
        {
            if (_disabledChords.Contains(combination.Chord)) return acc;
            // mark down_in_chord and continue try to find trigger
            if (type == KeyEventType.Down && combination.Chord.Contains(args.KeyCode)) downInChord = true;

            if (args.KeyCode != combination.TriggerKey || combination.Disabled) return acc;
            var mach = combination.Chord.All(args.KeyboardState.IsDown);
            if (!mach) return acc;
            if (acc == null) return combination;
            return acc.ChordLength >= combination.ChordLength ? acc : combination;
        });

        return new SelectionResult(this, candidateNode, downInChord);
    }

    //eventType is only Down or Up
    internal KeyProcessState Climb(KeyEventType eventTypeType, IKeyEventArgs args,
        TrieNode<ICombination, KeyEventCommand> candidateNode, bool downInChord)
    {
        Debug.Assert(args != null, nameof(args) + " != null");
        if (args.NoFurtherProcess) return ProcessState = KeyProcessState.NoFurtherProcess;

        // no match
        // Chord_downOrUp? or
        if (candidateNode == null)
        {
            if (eventTypeType == KeyEventType.Down)
            {
                if (_treeWalker.IsOnRoot)
                {
                    // AnyKeyNotInRoot_down_or_up: *A_down *A_up is not registered in root
                    _lastKeyDownNodeForAllUp = null;
                    return ProcessState = KeyProcessState.Yield;
                }

                //  KeyInChord_down:C+D, A+B A_down
                if (downInChord)
                    return ProcessState = KeyProcessState.Continue; // waiting for trigger key

                Reset();
                return ProcessState = KeyProcessState.Reprocess; // to process combination chord up
            }

            // allUp design goal:
            // 1. could register allUp event
            // 2. still navigate when A+B+C_up event not triggered because of chord_up before trigger_up
            if (_lastKeyDownNodeForAllUp != null &&
                _lastKeyDownNodeForAllUp.Key.IsAnyKey(args.KeyCode))
            {
                if (args.KeyboardState.AreAllUp(_lastKeyDownNodeForAllUp.Key.AllKeys))
                {
                    candidateNode = _lastKeyDownNodeForAllUp;
                    eventTypeType = KeyEventType.AllUp;
                }
                else
                {
                    return ProcessState = KeyProcessState.Continue;
                }
            }
            else
            {
                if (_treeWalker.IsOnRoot)
                {
                    // AnyKeyNotRegisteredInRoot_down_or_up: *A_down *A_up is not registered in root
                    _lastKeyDownNodeForAllUp = null;
                    return ProcessState = KeyProcessState.Yield;
                }

                // on path, up
                if (_treeWalker.CurrentChildrenCount == 0)
                {
                    // NoChild & NotOnRoot:
                    //   KeyInChord_up : A+B when A_up.
                    //   other keyup: A+B and B mapto C??
                    Reset();
                    return ProcessState = KeyProcessState.Reprocess; // Chord_up would be processed on root
                }

                // HaveChild & KeyInChord_up: A+B, C when A_up continue wait C
                if (_treeWalker.CurrentNode.Key.Chord.Contains(args.KeyCode))
                {
                    Console.WriteLine(
                        " would never been here:treeWalker.CurrentNode.Key.Chord.Contains(args.KeyCode)");
                    Debugger.Break();
                    return ProcessState = KeyProcessState.Continue;
                }

                //HaveChild & KeyNotInChord_up: B+D, F when C_up.
                Reset();
                return ProcessState = KeyProcessState.Reprocess;
            }
        }

        args.KeyEventType = eventTypeType;

        var lastDownHit = "";
        if (_lastKeyDownNodeForAllUp != null)
            lastDownHit = $"last↓@{_lastKeyDownNodeForAllUp}";
        Console.WriteLine($"${Name}{lastDownHit}");

        // matched
        var actionList = candidateNode.Values() as KeyActionList<KeyEventCommand>;
        Debug.Assert(actionList != null, nameof(actionList) + " != null");

        // execute
        var handled = candidateNode.Key.TriggerKey.Handled;
        var oneExecuted = false;
        foreach (var keyCommand in actionList[eventTypeType])
        {
            if (keyCommand.CanExecute != null && !keyCommand.CanExecute(args))
            {
                Console.WriteLine($"\t/!{eventTypeType}\t{keyCommand.Id}\t{keyCommand.Description}");
                continue;
            }

            oneExecuted = true;
            var execute = keyCommand.Execute;
            if ((eventTypeType & handled) != 0)
                args.Handled = true;
            var isAsync = execute?.Method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
            Console.WriteLine(
                $"\t!{eventTypeType}{(isAsync ? "_async" : "")}\t{keyCommand.Id}\t{keyCommand.Description}");
            try
            {
                execute?.Invoke(args);
                if (args.NoFurtherProcess)
                {
                    break;
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                Services.CommonLogger.LogError(e.ToString());
            }
        }

        if (!oneExecuted && actionList[eventTypeType].Any())
        {
            Console.WriteLine($"All event of type:{eventTypeType} not executable!");
            if (eventTypeType == KeyEventType.Up &&
                _lastKeyDownNodeForAllUp != null &&
                _lastKeyDownNodeForAllUp.Key.Chord.Contains(args.KeyCode))
            {
                return ProcessState = KeyProcessState.Continue;
            }

            Reset();
            return ProcessState = KeyProcessState.Yield; // all not executable, state of the eventType disabled
        }

        if (args.PathToGo != null && !args.PathToGo.SequenceEqual(candidateNode.KeyPath)) // goto state by requiring
        {
            if (!_treeWalker.TryGoToState(args.PathToGo, out var state))
            {
                Console.WriteLine($"Couldn't go to state {state}");
            }

            _lastKeyDownNodeForAllUp = null;
            return ProcessState = KeyProcessState.Continue;
        }
        // goto candidateNode
        switch (eventTypeType)
        {
            case KeyEventType.Up:
                {
                    // only navigate on up/AllUp event
                    _treeWalker.GoToChild(candidateNode);

                    if (candidateNode.ChildrenCount == 0)
                    {
                        if (actionList[KeyEventType.AllUp].Any())
                        {
                            // wait for chord up
                            return ProcessState = KeyProcessState.Continue;
                        }

                        _notify?.CloseKeysTip(Name);
                        Reset();
                        return ProcessState = KeyProcessState.Done;
                    }

                    _notify?.ShowKeysTip(Name, _treeWalker.CurrentNode.Tip);
                    return ProcessState = KeyProcessState.Continue;
                }

            case KeyEventType.AllUp:
                _lastKeyDownNodeForAllUp = null;
                // navigate on AllUp event only when not navigated by up
                // A+B down then B_up then A_up would not execute this if clause
                if (_treeWalker.CurrentNode.Equals(candidateNode))
                {
                    return ProcessState;
                }

                _treeWalker.GoToChild(candidateNode);

                if (candidateNode.ChildrenCount == 0)
                {
                    Reset();
                    _notify?.CloseKeysTip(Name);
                    return ProcessState = KeyProcessState.Done;
                }
                else
                {
                    _notify?.ShowKeysTip(Name, _treeWalker.CurrentNode.Tip);
                    return ProcessState = KeyProcessState.Continue;
                }

            case KeyEventType.Down:
                _lastKeyDownNodeForAllUp = candidateNode;
                return ProcessState = KeyProcessState.Continue;
            default:
                throw new Exception($"KeyEvent: {eventTypeType} not supported");
        }
    }
}