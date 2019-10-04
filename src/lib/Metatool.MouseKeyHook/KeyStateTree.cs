using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Metatool.DataStructures;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Input.MouseKeyHook.Implementation.Trie;
using Metatool.MetaKeyboard;

namespace Metatool.Input
{
    public enum KeyProcessState
    {
        /// <summary>
        ///  well processed this event and at root: event consumed, state reset
        /// </summary>
        Done,

        /// <summary>
        /// continue handling next event on the current state of the tree: event consumed, state kept
        /// </summary>
        Continue,

        /// <summary>
        /// reprocess this event on the root of trees, and the tree is at root: event reschedule(include me), state reset from path to root
        /// </summary>
        Reprocess,

        /// <summary>
        /// could not process the event, try to process this event with other machine, at root. event reschedule(exclude me), state reset
        /// </summary>
        Yield,

        /// <summary>
        /// stop further process for this event on any further tree: event consumed, state unknown
        /// </summary>
        NoFurtherProcess,
    }

    [DebuggerDisplay("${Name}")]
    public class KeyStateTree
    {
        public static readonly   KeyStateTree HardMap = new KeyStateTree("HardMap");
        internal static readonly KeyStateTree Default = new KeyStateTree("Default");
        public static readonly   KeyStateTree Map     = new KeyStateTree("Map");
        public static readonly KeyStateTree HotString = new KeyStateTree("HotString");

        static Dictionary<KeyStateTrees, KeyStateTree> stateTrees= new Dictionary<KeyStateTrees, KeyStateTree>()
        {
            { KeyStateTrees.Default, Default},
            {KeyStateTrees.HardMap, HardMap },
            {KeyStateTrees.Map, Map },
            {KeyStateTrees.HotString, HotString }
        };

        public static KeyStateTree GetStateTree(KeyStateTrees stateTree) => stateTrees[stateTree];

        private readonly Trie<ICombination, KeyEventCommand>       _trie = new Trie<ICombination, KeyEventCommand>();
        private readonly TrieWalker<ICombination, KeyEventCommand> _treeWalker;
        public           string                                    Name;

        internal KeyProcessState ProcessState;

        public KeyStateTree(string name)
        {
            Name                     = name;
            _treeWalker              = new TrieWalker<ICombination, KeyEventCommand>(_trie);
            _lastKeyDownNodeForAllUp = null;
        }

        internal TrieNode<ICombination, KeyEventCommand> CurrentNode => _treeWalker.CurrentNode;

        internal bool IsOnRoot => _treeWalker.IsOnRoot;

        public void Reset()
        {
            var lastDownHit = "";
            if (_lastKeyDownNodeForAllUp != null)
                lastDownHit = $"\t↓@ {_lastKeyDownNodeForAllUp}";
            _lastKeyDownNodeForAllUp = null;
            Console.WriteLine($"${Name}{lastDownHit}");

            Notify.CloseKeysTip(Name);
            _treeWalker.GoToRoot();
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

        private TrieNode<ICombination, KeyEventCommand> _lastKeyDownNodeForAllUp;

        internal struct SelectionResult
        {
            public SelectionResult(KeyStateTree tree, TrieNode<ICombination, KeyEventCommand> candidateNode,
                bool downInChord)
            {
                Tree          = tree;
                CandidateNode = candidateNode;
                DownInChord   = downInChord;
            }

            internal KeyStateTree                            Tree;
            internal TrieNode<ICombination, KeyEventCommand> CandidateNode;
            internal bool                                    DownInChord;
        }

        internal SelectionResult TrySelect(KeyEvent eventType, IKeyEventArgs args)
        {
            // to handle A+B+C(B is down in Chord)
            var downInChord = false;

            var type = eventType;
            var candidateNode = _treeWalker.GetChildOrNull((ICombination acc, ICombination combination) =>
            {
                // mark down_in_chord and continue try to find trigger
                if (type == KeyEvent.Down && combination.Chord.Contains(args.KeyCode)) downInChord = true;

                if (args.KeyCode != combination.TriggerKey || combination.Disabled) return acc;
                var mach = combination.Chord.All(args.KeyboardState.IsDown);
                if (!mach) return acc;
                if (acc == null) return combination;
                return acc.ChordLength >= combination.ChordLength ? acc : combination;
            });

            return new SelectionResult(this, candidateNode, downInChord);
        }

        internal KeyProcessState Climb(KeyEvent eventType, IKeyEventArgs iargs, TrieNode<ICombination, KeyEventCommand> candidateNode, bool downInChord)
        {
            var args = iargs as KeyEventArgsExt;
            Debug.Assert(args != null, nameof(args) + " != null");
            if (args.NoFurtherProcess) return ProcessState = KeyProcessState.NoFurtherProcess;

            // no match
            // Chord_downOrUp? or 
            if (candidateNode == null)
            {
                if (eventType == KeyEvent.Down)
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
                // 2. navigate when A+B+C_up event not triggered because of chord_up before trigger_up
                if (_lastKeyDownNodeForAllUp != null &&
                    _lastKeyDownNodeForAllUp.Key.IsAnyKey(args.KeyCode))
                {
                    if (args.KeyboardState.AreAllUp(_lastKeyDownNodeForAllUp.Key.AllKeys))
                    {
                        candidateNode = _lastKeyDownNodeForAllUp;
                        eventType     = KeyEvent.AllUp;
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
                        // AnyKeyNotInRoot_down_or_up: *A_down *A_up is not registered in root
                        _lastKeyDownNodeForAllUp = null;
                        return ProcessState = KeyProcessState.Yield;
                    }
                    // on path, up 
                    if (_treeWalker.ChildrenCount == 0)
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
                        Console.WriteLine(" would never been here:treeWalker.CurrentNode.Key.Chord.Contains(args.KeyCode)");
                        Debugger.Break();
                        return ProcessState = KeyProcessState.Continue;
                    }

                    //HaveChild & KeyNotInChord_up: B+D, F when C_up. 
                    Reset();
                    return ProcessState = KeyProcessState.Reprocess;
                }
            }

            args.KeyEvent = eventType;

            var lastDownHit = "";
            if (_lastKeyDownNodeForAllUp != null)
                lastDownHit = $"\t↓@{_lastKeyDownNodeForAllUp}";
            Console.WriteLine($"${Name}{lastDownHit}");

            // matched
            var actionList = candidateNode.Values() as KeyActionList<KeyEventCommand>;
            Debug.Assert(actionList != null, nameof(actionList) + " != null");

            // execute
#if !DEBUG
            try
            {
#endif
            var oneExecuted = false;
            foreach (var keyCommand in actionList[eventType])
            {

                if (keyCommand.CanExecute != null && !keyCommand.CanExecute(args))
                {
                    Console.WriteLine($"\t/!{eventType}\t{keyCommand.Name}\t{keyCommand.Description}");
                    continue;
                }

                oneExecuted = true;
                var exe     = keyCommand.Execute;
                var isAsync = exe?.Method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
                Console.WriteLine(
                    $"\t!{eventType}{(isAsync ? "_async" : "")}\t{keyCommand.Name}\t{keyCommand.Description}");
                exe?.Invoke(args);
            }

            if (!oneExecuted && actionList[eventType].Any())
            {
                Console.WriteLine($"All event of type:{eventType} not executable!");
                if (eventType                == KeyEvent.Up &&
                    _lastKeyDownNodeForAllUp != null        &&
                    _lastKeyDownNodeForAllUp.Key.Chord.Contains(args.KeyCode))
                {
                    return ProcessState = KeyProcessState.Continue;
                }

                Reset();
                return ProcessState = KeyProcessState.Yield; // all not executable, state of the eventType disabled
            }
#if !DEBUG
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
#endif
            if (args.PathToGo != null && !args.PathToGo.SequenceEqual(candidateNode.KeyPath)) // goto state by requiring
            {
                if (!_treeWalker.TryGoToState(args.PathToGo, out var state))
                {
                    Console.WriteLine($"Couldn't go to state {state}");
                }

                _lastKeyDownNodeForAllUp = null;
                return ProcessState = KeyProcessState.Continue;
            }

            switch (eventType)
            {
                case KeyEvent.Up:
                {
                    // only navigate on up/AllUp event
                    _treeWalker.GoToChild(candidateNode);

                    if (candidateNode.ChildrenCount == 0)
                    {
                        if (actionList[KeyEvent.AllUp].Any())
                        {
                            // wait for chord up
                            return ProcessState = KeyProcessState.Continue;
                        }

                        Notify.CloseKeysTip(Name);
                        Reset();
                        return ProcessState = KeyProcessState.Done;
                    }

                    Notify.ShowKeysTip(Name, _treeWalker.CurrentNode.Tip);
                    return ProcessState = KeyProcessState.Continue;
                }

                case KeyEvent.AllUp:
                    _lastKeyDownNodeForAllUp = null;
                    // navigate on AllUp event only when not navigated by up
                    // A+B then B_up then A_up would not execute if clause
                    if (_treeWalker.CurrentNode.Equals(candidateNode))
                    {
                        return ProcessState;
                    }

                    _treeWalker.GoToChild(candidateNode);

                    if (candidateNode.ChildrenCount == 0)
                    {
                        Reset();
                        Notify.CloseKeysTip(Name);
                        return ProcessState = KeyProcessState.Done;
                    }
                    else
                    {
                        Notify.ShowKeysTip(Name, _treeWalker.CurrentNode.Tip);
                        return ProcessState = KeyProcessState.Continue;
                    }

                case KeyEvent.Down:
                    _lastKeyDownNodeForAllUp = candidateNode;
                    return ProcessState = KeyProcessState.Continue;
                default:
                    throw new Exception($"KeyEvent: {eventType} not supported");
            }
        }
    }
}
