using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// continue handling next event on the current state
        /// </summary>
        Continue,

        /// <summary>
        /// reprocess this event on the root of trees, and the tree is at root
        /// </summary>
        Reprocess,

        /// <summary>
        /// could not process the event, try to process this event with other machine.
        /// </summary>
        Yield,

        /// <summary>
        /// stop further process for this event on any further tree
        /// </summary>
        NoFurtherProcess,

    }

    public class KeyStateTree
    {
        internal static readonly KeyStateTree Default = new KeyStateTree("Default");
        public static readonly  KeyStateTree HardMap = new KeyStateTree("HardMap");
        public static readonly  KeyStateTree Map     = new KeyStateTree("Map");

        private readonly Trie<ICombination, KeyEventCommand>       _trie = new Trie<ICombination, KeyEventCommand>();
        private readonly TrieWalker<ICombination, KeyEventCommand> _treeWalker;
        public           string                                   Name;

        public KeyStateTree(string name)
        {
            Name                      = name;
            _treeWalker              = new TrieWalker<ICombination, KeyEventCommand>(_trie);
            _lastKeyDownNode_ForAllUp = null;
        }

        public void Reset()
        {
            var lastDownHit = "";
            if (_lastKeyDownNode_ForAllUp != null)
                lastDownHit = $"\t@↓{_lastKeyDownNode_ForAllUp}";
            Console.WriteLine($"${Name}{lastDownHit}");

            Notify.CloseKeysTip(Name);
            _treeWalker.GoToRoot();
        }


        public IEnumerable<(string key, IEnumerable<string> descriptions)> Tips(bool ifRootThenEmpty = false)
        {
            if (ifRootThenEmpty && _treeWalker.CurrentNode == _treeWalker.Root)
            {
                return Enumerable.Empty<(string key, IEnumerable<string> descriptions)>();
            }

            return _treeWalker.CurrentNode.Tip;
        }

        public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command)
        {
            _trie.Add(combinations, command);
            return new MetaKey(_trie, combinations, command);
        }

        public IMetaKey Add(ICombination combination, KeyEventCommand command)
        {
            return Add(new List<ICombination> {combination}, command);
        }

        private       TrieNode<ICombination, KeyEventCommand> _lastKeyDownNode_ForAllUp;

        public KeyProcessState ProcessKeyEvent(KeyEvent eventType, KeyEventArgsExt args)
        {


            if (args.NoFurtherProcess) return KeyProcessState.NoFurtherProcess;
            // to handle A+B+C(B is down in Chord)
            var downInChord = false;

            var type = eventType;
            var childNode = _treeWalker.GetChildOrNull((ICombination acc, ICombination combination) =>
            {
                // mark down_in_chord and continue try to find trigger
                if (type == KeyEvent.Down && combination.Chord.Contains(args.KeyCode)) downInChord = true;

                if ( args.KeyCode != combination.TriggerKey || combination.Disabled ) return acc;
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
                    if (_treeWalker.IsOnRoot)
                    {
                        _lastKeyDownNode_ForAllUp = null;
                        return KeyProcessState.Yield;
                    }

                    //  KeyInChord_down:C+D, A+B A_down
                    if (downInChord)
                        return KeyProcessState.Continue; // waiting for trigger key

                    Reset();
                    return KeyProcessState.Reprocess; // to process combination chord up
                }
                else // Chord_up or AnyKeyNotInRoot_up
                {
                    // A+B+C_down then release chord or trigger in any sequence n
                    if (_lastKeyDownNode_ForAllUp != null &&
                        _lastKeyDownNode_ForAllUp.Key.IsAnyKey(args.KeyCode))
                    {
                        if (args.KeyboardState.AreAllUp(_lastKeyDownNode_ForAllUp.Key.AllKeys))
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
                        if (_treeWalker.IsOnRoot)
                        {
                            _lastKeyDownNode_ForAllUp = null;
                            return KeyProcessState.Yield;
                        }

                        if (_treeWalker.ChildrenCount == 0)
                        {
                            // NoChild & NotOnRoot:
                            //   KeyInChord_up : A+B when A_up. if A mapto C, A_up -> C_up
                            //   other keyup: A+B and B mapto C??
                            Reset();
                            return KeyProcessState.Reprocess; // to process combination chord up
                        }
                        else
                        {
                            // KeyInChord_up & haveChild: A+B, C when A_up continue wait C
                            // should we also process the Chord_up at root?
                            if (_treeWalker.CurrentNode.Key.Chord.Contains(args.KeyCode))
                            {
                                return KeyProcessState.Continue; // combination chord keys up, to process child
                            }

                            // KeyNotInChord_up & HaveChild: B+D, F when C_up. if B mapto C, B_up -> C_up
                            Reset();
                            return KeyProcessState.Reprocess; // to process combination chord up
                        }
                    }
                }
            }

            var lastDownHit = "";
            if (_lastKeyDownNode_ForAllUp != null)
                lastDownHit = $"\t@↓{_lastKeyDownNode_ForAllUp}";
            Console.WriteLine($"${Name}{lastDownHit}");

            // matched
            var actionList = childNode.Values() as KeyActionList<KeyEventCommand>;
            Debug.Assert(actionList != null, nameof(actionList) + " != null");

            // execute
#if !DEBUG
            try
            {
#endif
            (childNode.Key as Combination)?.OnEvent(args);
            foreach (var keyEventAction in actionList[eventType])
            {
                if(keyEventAction.CanExecute !=null && !keyEventAction.CanExecute(args)) continue;

                var exe = keyEventAction.Execute;
                var isAsync = exe?.Method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
                Console.WriteLine($"\t!{eventType}{(isAsync ? "_async":"")}\t{keyEventAction.Id}\t{keyEventAction.Description}");
                if (isAsync) args.BeginInvoke(exe);
                else exe?.Invoke(args);
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
                if (!_treeWalker.TryGoToState(args.PathToGo, out var state))
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
                    _treeWalker.GoToChild(childNode);

                    if (childNode.ChildrenCount == 0)
                    {
                        if (actionList[KeyEvent.AllUp].Any())
                        {
                            // wait for chord up
                            return KeyProcessState.Continue;
                        }

                        Notify.CloseKeysTip(Name);
                        Reset();
                        _lastKeyDownNode_ForAllUp = null;
                        return KeyProcessState.Done;
                    }
                    else
                    {
                        Notify.ShowKeysTip(Name, _treeWalker.CurrentNode.Tip);
                        return KeyProcessState.Continue;
                    }
                }

                case KeyEvent.AllUp:
                    _lastKeyDownNode_ForAllUp = null;
                    // navigate on AllUp event only when not navigated by up
                    // A+B then B_up then A_up would not execute if clause
                    if (_treeWalker.CurrentNode.Equals(childNode.Parent))
                    {
                        _treeWalker.GoToChild(childNode);
                        if (childNode.ChildrenCount == 0)
                        {
                            Notify.CloseKeysTip(Name);
                            Reset();
                        }
                        else
                        {
                            Notify.ShowKeysTip(Name,_treeWalker.CurrentNode.Tip);
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