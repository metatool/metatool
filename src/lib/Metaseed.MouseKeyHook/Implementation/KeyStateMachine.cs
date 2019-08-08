﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.Input.MouseKeyHook.Implementation.Trie;
using Metaseed.MetaKeyboard;
using OneOf;

namespace Metaseed.Input
{
    using HotKey = OneOf<ISequenceUnit, ISequence>;

    public enum KeyProcessState
    {
        /// <summary>
        ///  well processed this event and at root
        /// </summary>
        Done,

        /// <summary>
        /// continue handling next event on the current machine
        /// </summary>
        Continue,

        /// <summary>
        /// reprocess this event on the current machine
        /// </summary>
        Reset,

        /// <summary>
        /// try to process this event with other machine.
        /// </summary>
        Yield,

        /// <summary>
        /// Max Recursive Count
        /// </summary>
        MaxRecursive
    }

    public class KeyStateMachine
    {
        private readonly Trie<ICombination, KeyEventAction>       _trie = new Trie<ICombination, KeyEventAction>();
        private readonly TrieWalker<ICombination, KeyEventAction> _stateWalker;
        public           string                                   Name;

        public KeyStateMachine(string name)
        {
            Name                      = name;
            _stateWalker              = new TrieWalker<ICombination, KeyEventAction>(_trie);
            _lastKeyDownNode_ForAllUp = null;
        }

        public void Reset()
        {
            Notify.CloseKeysTip(Name);
            _stateWalker.GoToRoot();
        }

        public void Reset(KeyEvent eventType, KeyEventArgsExt args)
        {
            Reset();
            recursiveCount++;
            KeyEventProcess(eventType, args);
            recursiveCount--;
        }

        public IEnumerable<(string key, IEnumerable<string> descriptions)> Tips(bool ifRootThenEmpty = false)
        {
            if (ifRootThenEmpty && _stateWalker.CurrentNode == _stateWalker.Root)
            {
                return Enumerable.Empty<(string key, IEnumerable<string> descriptions)>();
            }

            return _stateWalker.CurrentNode.Tip;
        }

        public IMetaKey Add(IList<ICombination> combinations, KeyEventAction action)
        {
            _trie.Add(combinations, action);
            return new MetaKey(_trie, combinations, action);
        }

        public IMetaKey Add(ICombination combination, KeyEventAction action)
        {
            return Add(new List<ICombination> {combination}, action);
        }

        private const int                                    MaxRecursiveCount = 50;
        private       int                                    recursiveCount    = 0;
        private       TrieNode<ICombination, KeyEventAction> _lastKeyDownNode_ForAllUp;

        public KeyProcessState KeyEventProcess(KeyEvent eventType, KeyEventArgsExt args)
        {
            var lastDownHit = "";
            if (_lastKeyDownNode_ForAllUp != null)
                lastDownHit = $"\t@↓{_lastKeyDownNode_ForAllUp}";
            Console.WriteLine($"${Name}{lastDownHit}"); // to trace recurrent
            if (recursiveCount > MaxRecursiveCount)
            {
                Console.WriteLine($"\tRecursiveCount: {recursiveCount}>{MaxRecursiveCount}");
                recursiveCount            = 0;
                _lastKeyDownNode_ForAllUp = null;
                return KeyProcessState.MaxRecursive;
            }

            // to handle A+B+C(B is down in Chord)
            var downInChord = false;

            var type = eventType;
            var childNode = _stateWalker.GetChildOrNull((ICombination acc, ICombination combination) =>
            {
                // mark down_in_chord and continue try to find trigger
                if (type == KeyEvent.Down && combination.Chord.Contains(args.KeyCode)) downInChord = true;

                if (combination.Disabled || args.KeyCode != combination.TriggerKey) return acc;
                var mach = combination.Chord.All(args.KeyboardState.IsDown);
                if (!mach) return acc;
                if (acc == null) return combination;
                return acc.ChordLength >= combination.ChordLength ? acc : combination;
            });

            // no match
            // Chord_downOrUp? or 
            if (childNode == null)
            {
                if (eventType == KeyEvent.Down)
                {
                    // AnyKeyNotInRoot_down: *A_down is not registered in root
                    if (_stateWalker.IsOnRoot)
                    {
                        _lastKeyDownNode_ForAllUp = null;
                        return KeyProcessState.Yield;
                    }

                    //  KeyInChord_down: A+B A_down
                    if (downInChord)
                        return KeyProcessState.Continue; // waiting for trigger key

                    Reset(eventType, args);
                    return KeyProcessState.Reset; // to process combination chord up
                }
                else // Chord_up or AnyKeyNotInRoot_up
                {
                    // A+B+C_down then release chord or trigger in any sequence n
                    if (_lastKeyDownNode_ForAllUp != null &&
                        _lastKeyDownNode_ForAllUp.Key.IsAnyKey(args.KeyCode))
                    {
                        if (args.KeyboardState.IsUp(_lastKeyDownNode_ForAllUp.Key.TriggerKey) &&
                            args.KeyboardState.AreAllUp(_lastKeyDownNode_ForAllUp.Key.Chord))
                        {
                            childNode = _lastKeyDownNode_ForAllUp;
                            eventType = KeyEvent.AllUp;
                        }
                        else
                        {
                            return KeyProcessState.Continue;
                        }
                    }
                    else // not any key_up in last down
                    {
                        // AnyKeyNotInRoot_up: *A_up is not registered in root
                        if (_stateWalker.IsOnRoot)
                        {
                            _lastKeyDownNode_ForAllUp = null;
                            return KeyProcessState.Yield;
                        }

                        if (_stateWalker.ChildrenCount == 0)
                        {
                            // NoChild & NotOnRoot:
                            //   KeyInChord_up : A+B when A_up. if A mapto C, A_up -> C_up
                            //   other keyup: A+B and B mapto C??
                            Reset(eventType, args);
                            return KeyProcessState.Reset; // to process combination chord up
                        }
                        else
                        {
                            // KeyInChord_up & haveChild: A+B, C when A_up continue wait C
                            // should we also process the Chord_up at root?
                            if (_stateWalker.CurrentNode.Key.Chord.Contains(args.KeyCode))
                            {
                                return KeyProcessState.Continue; // combination chord keys up, to process child
                            }

                            // KeyNotInChord_up & HaveChild: B+D, F when C_up. if B mapto C, A_up -> C_up
                            Reset(eventType, args);
                            return KeyProcessState.Reset; // to process combination chord up
                        }
                    }
                }
            }

            // matched
            var actionList = childNode.Values() as KeyActionList<KeyEventAction>;
            Debug.Assert(actionList != null, nameof(actionList) + " != null");

            // execute
#if !DEBUG
            try
            {
#endif
            (childNode.Key as Combination)?.OnEvent(args);
            foreach (var keyEventAction in actionList[eventType])
            {
                Console.WriteLine($"\t!{eventType}\t{keyEventAction.Id}\t{keyEventAction.Description}");
                keyEventAction.Execute?.Invoke(args);
            }
#if !DEBUG
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
#endif
            if (args.PathToGo != null && !args.PathToGo.SequenceEqual(childNode.KeyPath)) // goto state by requiring
            {
                if (!_stateWalker.TryGoToState(args.PathToGo, out var state))
                {
                    Console.WriteLine($"Couldn't go to state {state}");
                }

                _lastKeyDownNode_ForAllUp = null;
                return KeyProcessState.Continue;
            }

            switch (eventType)
            {
                case KeyEvent.Up:
                {
                    // only navigate on up/AllUp event
                    _stateWalker.GoToChild(childNode);

                    if (childNode.ChildrenCount == 0)
                    {
                        if (actionList[KeyEvent.AllUp].Any())
                        {
                            return KeyProcessState.Continue;
                        }

                        Notify.CloseKeysTip(Name);
                        Reset();
                        _lastKeyDownNode_ForAllUp = null;
                        return KeyProcessState.Done;
                    }
                    else
                    {
                        Notify.ShowKeysTip(Name, _stateWalker.CurrentNode.Tip);
                        return KeyProcessState.Continue;
                    }
                }

                case KeyEvent.AllUp:
                    _lastKeyDownNode_ForAllUp = null;
                    // navigate on AllUp event only when not navigated by up
                    if (_stateWalker.CurrentNode.Equals(childNode.Parent))
                    {
                        _stateWalker.GoToChild(childNode);
                        if (childNode.ChildrenCount == 0)
                        {
                            Notify.CloseKeysTip(Name);
                            Reset();
                        }
                        else
                        {
                            Notify.ShowKeysTip(Name,_stateWalker.CurrentNode.Tip);
                            return KeyProcessState.Continue;
                        }
                    }

                    return KeyProcessState.Done;
                case KeyEvent.Down:
                    _lastKeyDownNode_ForAllUp = childNode;
                    return KeyProcessState.Continue;
                default:
                    throw new Exception($"KeyEvent: {eventType} not supported");
            }
        }
    }
}