﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;

namespace Metatool.Input;

[DebuggerDisplay("${Name}")]
public class KeyStateTree(string name, IKeyTipNotifier notify)
{
    public TreeType TreeType = TreeType.Default;

    private readonly Trie<ICombination, KeyEventCommand> _trie = new();
    public string Name = name;

    internal TreeClimbingState ClimbingState;

    internal TrieNode<ICombination, KeyEventCommand> CurrentNode => _trie.CurrentNode;

    internal bool IsOnRoot => _trie.IsOnRoot;

    public void Reset()
    {
        var lastDownHit = "";

        if (_lastKeyDownNodeForAllUp != null)
            lastDownHit = $"last↓@ {_lastKeyDownNodeForAllUp}";
        _lastKeyDownNodeForAllUp = null;

        Console.WriteLine($"${Name}{lastDownHit}");

        Task.Run(() => notify?.CloseKeysTip(Name)); // use task here, because the slow startup
        _trie.GoToRoot();
    }

    internal bool Contains(IHotkey hotKey)
    {
        var values = hotKey switch
        {
            ISequenceUnit k => _trie.TryGet([.. k.ToCombination()], out _),
            ISequence s => _trie.TryGet(s.ToList(), out _),
            _ => throw new Exception("not supported!")
        };

        return values;
    }

    internal void MarkDoneIfLanding()
    {
        if (ClimbingState == TreeClimbingState.Landing)
        {
            ClimbingState = TreeClimbingState.Done;
            Console.WriteLine($"${Name}@Landing->@Done");
        }
    }

    public IEnumerable<(string key, IEnumerable<string> descriptions)> Tips(bool ifRootThenEmpty = false)
    {
        if (ifRootThenEmpty && _trie.IsOnRoot)
        {
            return [];
        }

        return _trie.CurrentNode.Tip;
    }

    public IMetaKey Add(IList<ICombination> path, KeyEventCommand command)
    {
        if (TreeType == TreeType.SingleFruitPerEventType)
        {
            _trie.TryGet(path, out var node);
            if (node != null)
            {
                _trie.Remove(path, c => c.KeyEventType == command.KeyEventType);
            }
        }

        _trie.Add(path, command);
        return new MetaKey(_trie, path, command);
    }

    private TrieNode<ICombination, KeyEventCommand>? _lastKeyDownNodeForAllUp = null;

    /// <summary>
    /// these chords are disabled, the key can not be used in the chord part of combination
    /// </summary>
    private readonly HashSet<Chord> _disabledChords = [];

    internal void DisableChord(Chord chord)
    {
        _disabledChords.Add(chord);
    }

    internal void EnableChord(Chord chord)
    {
        _disabledChords.Remove(chord);
    }

    /// <summary>
    /// with the key event, try to find the best matching child node from current node
    /// best matching: chord is not disabled in current tree, is not marked disabled in tree node, chord+trigger are all down, the one with most chord keys down.
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal SelectionResult TrySelect(KeyEventType eventType, IKeyEventArgs args)
    {
        // to handle A+B+C(B is currently down in Chord)
        var downInChord = false;
        ICombination? candidate = null;
        foreach (var childKey in _trie.CurrentNode.ChildrenDictionary.Keys)
        {
            if (_disabledChords.Contains(childKey.Chord))
                continue;

            // mark down_in_chord and continue try to find trigger
            // todo: no other key not in chord down: A+B+C, if D is down too, this is not considered next. means only exact chord mach will trigger the hotkey
            if (eventType == KeyEventType.Down && childKey.Chord.Contains(args.KeyCode))
                downInChord = true;

            if (args.KeyCode != childKey.TriggerKey || childKey.Disabled)
                continue;
            // A+B and A+B+C, and B is down, so A+B will be selected
            // here: current's trigger key down
            var allChordDown = childKey.Chord.All(args.KeyboardState.IsDown);
            if (!allChordDown)
                continue;

            // here: current's all chord is down
            if (candidate == null)
            {
                candidate = childKey;
                continue;
            }
            // select the one with most chord keys down: A+B+C vs A+C -> A+B+C when A and B are down
            if (candidate.ChordCount < childKey.ChordCount)
            {
                candidate = childKey;
            }
        }

        if (candidate != null)
        {
            _trie.CurrentNode.ChildrenDictionary.TryGetValue(candidate, out var candidateNode);
            return new SelectionResult(this, candidateNode, downInChord);
        }

        return new SelectionResult(this, null, downInChord);
    }

    //eventType is only Down or Up
    internal TreeClimbingState Climb(KeyEventType eventType, IKeyEventArgs args, TrieNode<ICombination, KeyEventCommand> candidateNode, bool downInChord)
    {
        Debug.Assert(args != null, nameof(args) + " != null");

        if (args.NoFurtherProcess)
            return ClimbingState = TreeClimbingState.NoFurtherProcess;

        // no match
        // Chord_downOrUp? or
        if (candidateNode == null)
        {
            if (eventType == KeyEventType.Down)
            {
                if (_trie.IsOnRoot)
                {
                    // AnyKeyNotInRoot_down_or_up: *A_down *A_up is not registered in root
                    _lastKeyDownNodeForAllUp = null;
                    return ClimbingState = TreeClimbingState.Landing;
                }

                //  KeyInChord_down:C+D, A+B A_down
                if (downInChord)
                    return ClimbingState = TreeClimbingState.Continue; // waiting for trigger key

                Reset();
                return ClimbingState = TreeClimbingState.LandingAndClimbing; // to process combination chord up
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
                    eventType = KeyEventType.AllUp;
                }
                else
                {
                    return ClimbingState = TreeClimbingState.Continue;
                }
            }
            else
            {
                if (_trie.IsOnRoot)
                {
                    // AnyKeyNotRegisteredInRoot_down_or_up: *A_down *A_up is not registered in root
                    _lastKeyDownNodeForAllUp = null;
                    return ClimbingState = TreeClimbingState.Landing;
                }

                // on path, up
                if (_trie.CurrentNode.ChildrenDictionary.Count == 0)
                {
                    // NoChild & NotOnRoot:
                    //   KeyInChord_up : A+B when A_up.
                    //   other keyup: A+B and B map to C??
                    Reset();
                    return ClimbingState = TreeClimbingState.LandingAndClimbing; // Chord_up would be processed on root
                }

                // HaveChild & KeyInChord_up: A+B, C when A_up continue wait C
                if (_trie.CurrentNode.Key.Chord.Contains(args.KeyCode))
                {
                    Console.WriteLine(
                        " would never been here:treeWalker.CurrentNode.Key.Chord.Contains(args.KeyCode)");
                    Debugger.Break();
                    return ClimbingState = TreeClimbingState.Continue;
                }

                //HaveChild & KeyNotInChord_up: B+D, F when C_up.
                Reset();
                return ClimbingState = TreeClimbingState.LandingAndClimbing;
            }
        }

        args.KeyEventType = eventType;

        var lastDownHit = "";
        if (_lastKeyDownNodeForAllUp != null)
            lastDownHit = $"last↓@{_lastKeyDownNodeForAllUp}";
        Console.WriteLine($"${Name}{lastDownHit}");

        // matched
        var actionList = candidateNode.Values as KeyActionList<KeyEventCommand>;
        Debug.Assert(actionList != null, nameof(actionList) + " != null");

        // execute
        var handled = candidateNode.Key.TriggerKey.Handled;
        var oneExecuted = false;
        foreach (var keyCommand in actionList[eventType])
        {
            if (keyCommand.CanExecute != null && !keyCommand.CanExecute(args))
            {
                Console.WriteLine($"\t/!{eventType}\t{keyCommand.Id}\t{keyCommand.Description}");
                continue;
            }

            oneExecuted = true;
            var execute = keyCommand.Execute;
            if ((eventType & handled) != 0)
                args.Handled = true;
            var isAsync = execute?.Method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
            Console.WriteLine(
                $"\t!{eventType}{(isAsync ? "_async" : "")}\t{keyCommand.Id}\t{keyCommand.Description}");
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

        if (!oneExecuted && actionList[eventType].Any())
        {
            Console.WriteLine($"All event of type:{eventType} not executable!");
            if (eventType == KeyEventType.Up &&
                _lastKeyDownNodeForAllUp != null &&
                _lastKeyDownNodeForAllUp.Key.Chord.Contains(args.KeyCode))
            {
                return ClimbingState = TreeClimbingState.Continue;
            }

            Reset();
            return ClimbingState = TreeClimbingState.Landing; // all not executable, state of the eventType disabled
        }

        if (args.PathToGo != null && !args.PathToGo.SequenceEqual(candidateNode.KeyPath)) // goto state by requiring
        {
            if (!_trie.TryGoTo(args.PathToGo.ToList(), out var state))
            {
                Console.WriteLine($"Couldn't go to state {state}");
            }

            _lastKeyDownNodeForAllUp = null;
            return ClimbingState = TreeClimbingState.Continue;
        }
        // goto candidateNode
        switch (eventType)
        {
            case KeyEventType.Up:
                {
                    // only navigate on up/AllUp event
                    _trie.CurrentNode = candidateNode;

                    if (candidateNode.ChildrenDictionary.Count == 0)
                    {
                        if (actionList[KeyEventType.AllUp].Any())
                        {
                            // wait for chord up
                            return ClimbingState = TreeClimbingState.Continue;
                        }

                        notify?.CloseKeysTip(Name);
                        Reset();
                        return ClimbingState = TreeClimbingState.Done;
                    }

                    notify?.ShowKeysTip(Name, _trie.CurrentNode.Tip);
                    return ClimbingState = TreeClimbingState.Continue;
                }

            case KeyEventType.AllUp:
                _lastKeyDownNodeForAllUp = null;
                // navigate on AllUp event only when not navigated by up
                // A+B down then B_up then A_up would not execute this if-clause
                if (_trie.CurrentNode.Equals(candidateNode))
                {
                    return ClimbingState;
                }

                _trie.CurrentNode = candidateNode;

                if (candidateNode.ChildrenDictionary.Count == 0)
                {
                    Reset();
                    notify?.CloseKeysTip(Name);
                    return ClimbingState = TreeClimbingState.Done;
                }

                notify?.ShowKeysTip(Name, _trie.CurrentNode.Tip);
                return ClimbingState = TreeClimbingState.Continue;

            case KeyEventType.Down:
                _lastKeyDownNodeForAllUp = candidateNode;
                return ClimbingState = TreeClimbingState.Continue;

            default:
                throw new Exception($"KeyEvent: {eventType} not supported");
        }
    }
}