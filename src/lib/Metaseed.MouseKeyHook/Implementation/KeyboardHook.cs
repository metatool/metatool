using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.Input.MouseKeyHook.Implementation.Trie;
using Metaseed.MetaKeyboard;
using OneOf;

namespace Metaseed.Input.MouseKeyHook
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public delegate void KeyEventHandler(object sender, KeyEventArgsExt e);

    public class KeyboardHook
    {
        private readonly IKeyboardMouseEvents _eventSource;


        private readonly List<KeyStateTree> _stateMachines = new List<KeyStateTree>()
            {KeyStateTree.HardMap, KeyStateTree.Default, KeyStateTree.Map};

        public KeyboardHook()
        {
            _eventSource = Hook.GlobalEvents();
        }

        public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command,
            KeyStateTree keyStateTree = null)
        {
            var stateMachine = keyStateTree ?? KeyStateTree.Default;
            if (!_stateMachines.Contains(stateMachine)) _stateMachines.Add(stateMachine);
            return stateMachine.Add(combinations, command);
        }

        private readonly List<KeyEventHandler> _keyUpHandlers = new List<KeyEventHandler>();

        public event KeyEventHandler KeyUp
        {
            add => _keyUpHandlers.Add(value);
            remove => _keyUpHandlers.Remove(value);
        }

        private readonly List<KeyPressEventHandler> _keyPressHandlers = new List<KeyPressEventHandler>();

        public event KeyPressEventHandler KeyPress
        {
            add => _keyPressHandlers.Add(value);
            remove => _keyPressHandlers.Remove(value);
        }

        private readonly List<KeyEventHandler> _keyDownHandlers = new List<KeyEventHandler>();

        public event KeyEventHandler KeyDown
        {
            add => _keyDownHandlers.Add(value);
            remove => _keyDownHandlers.Remove(value);
        }

        public void ShowTip(bool ifRootThenEmpty = false)
        {
            //if (_currentMachine != null) Notify.ShowKeysTip(_currentMachine.Tips);
            var tips = _stateMachines.SelectMany(m => m.Tips(ifRootThenEmpty)).ToArray();
            if (tips.Length > 0)
                Notify.ShowKeysTip(tips);
            else
            {
                Notify.CloseKeysTip();
            }
        }

        private const int MaxRecursiveCount = 50;

        public void Run()
        {
            _stateMachines.ForEach(m => m.Reset());

            var selectedNodes =
                new List<(KeyStateTree tree, TrieNode<ICombination, KeyEventCommand> node, bool downInChord)
                >();


            void ClimbTree(KeyEvent eventType, KeyEventArgsExt args)
            {
                //Q: what if key sent in other machine, and we are on the keypath
                //A: we could use is virtual key filter
                // foreach (var keyStateMachine in _stateMachines)
                // {
                //     var result = keyStateMachine.KeyEventProcess(eventType, args);
                //     if (result == KeyProcessState.NoFurtherProcess)
                //         return;
                // }


                // if machine_1 has A+B and machine_2's A and B, press A+B on machine_1 would be processed
                // if machine_1 has A and machine_2 has A, both should be processed.
                // // continue process this event on current machine

                do
                {
                    var reprocess = false;
                    var hasSelectedNodes = selectedNodes.Count > 0;
                    foreach (var c in selectedNodes.GetRange(0, selectedNodes.Count))
                    {
                        var r = c.tree.TryClimb(eventType, args);
                        selectedNodes[selectedNodes.IndexOf(c)] = r;

                        var rt = r.tree.Climb(eventType, args, r.node, r.downInChord);
                        Console.WriteLine($"\t={rt}@{c.tree.Name}");
                        if (rt == KeyProcessState.Continue)
                        {
                        }
                        else if (rt == KeyProcessState.Done)
                        {
                            selectedNodes.Remove(c);
                        }
                        else if (rt == KeyProcessState.NoFurtherProcess)
                        {
                            selectedNodes.Remove(c);
                            return;
                        }
                        else if (rt == KeyProcessState.Reprocess || rt == KeyProcessState.Yield)
                        {
                            selectedNodes.Remove(c);
                            reprocess = true;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }


                    if (selectedNodes.Count > 0 || !reprocess && hasSelectedNodes)
                    {
                        return; // Continue on branch
                    }
                    // Reprocess or Yield, the Yield state on tree is valid, we are not on tree 

                    //all on root, find current trees
                    foreach (var stateTree in _stateMachines)
                    {
                        // if (stateTree.State == KeyProcessState.Yield) continue;

                        var candidate = stateTree.TryClimb(eventType, args);
                        if (candidate.node == null) continue;

                        if (selectedNodes.Count            == 0 ||
                            candidate.node.Key.ChordLength == selectedNodes[0].node.Key.ChordLength)
                        {
                            selectedNodes.Add(candidate);
                        }
                        else if (candidate.node.Key.ChordLength > selectedNodes[0].node.Key.ChordLength)
                        {
                            selectedNodes.Clear();
                            selectedNodes.Add(candidate);
                        }
                    }

                    if (selectedNodes.Count > 0)
                        Console.WriteLine(
                            $"\tToClimb:{string.Join(",", selectedNodes.Select(t => "$" + t.tree.Name))}");
                } while (selectedNodes.Count > 0);
            }

            _eventSource.KeyDown += (sender, args) =>
                ClimbTree(KeyEvent.Down, args as KeyEventArgsExt);
            _eventSource.KeyUp += (sender, args) =>
                ClimbTree(KeyEvent.Up, args as KeyEventArgsExt);

            _keyDownHandlers.ForEach(h => _eventSource.KeyDown += h);
            _keyUpHandlers.ForEach(h => _eventSource.KeyUp     += h);
        }
    }
}