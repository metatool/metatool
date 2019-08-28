using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.MetaKeyboard;
using OneOf;

namespace Metaseed.Input.MouseKeyHook
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public delegate void KeyEventHandler(object sender, KeyEventArgsExt e);

    public class KeyboardHook
    {
        private readonly IKeyboardMouseEvents _eventSource;


        private readonly List<KeyStateTree> _stateTrees = new List<KeyStateTree>()
            {KeyStateTree.HardMap, KeyStateTree.Default, KeyStateTree.Map, KeyStateTree.HotString};

        public KeyboardHook()
        {
            _eventSource = Hook.GlobalEvents();
        }

        public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command,
            KeyStateTree keyStateTree = null)
        {
            var stateMachine = keyStateTree ?? KeyStateTree.Default;
            if (!_stateTrees.Contains(stateMachine)) _stateTrees.Add(stateMachine);
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
            var tips = _stateTrees.SelectMany(m => m.Tips(ifRootThenEmpty)).ToArray();
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
            _stateTrees.ForEach(m => m.Reset());

            var selectTrees = new List<KeyStateTree.SelectionResult>();

            void ClimbTree(KeyEvent eventType, KeyEventArgsExt args)
            {
                // if machine_1 has A+B and machine_2's A and B, press A+B on machine_1 would be processed
                // if machine_1 has A and machine_2 has A, both should be processed.
                // runs like all are in the same tree, but provide state jump for every tree
                // // continue process this event on current machine
                bool hasSelectedNodes;
                bool reprocess;
                do
                {
                    reprocess = false;
                    var onGround = false;
                    if (selectTrees.Count == 0)
                    {
                        onGround    = true;
                        selectTrees = SelectTree(eventType, args);
                    }

                    hasSelectedNodes = selectTrees.Count > 0;
                    if (selectTrees.Count <= 0) goto @return;

                    foreach (var c in selectTrees.GetRange(0, selectTrees.Count))
                    {
                        var selectResult = c;
                        if (!onGround)
                        {
                            var r = selectResult.Tree.TrySelect(eventType, args);
                            selectTrees[selectTrees.IndexOf(selectResult)] = r;
                            selectResult                                   = r;
                        }

                        var rt = selectResult.Tree.Climb(eventType, args, selectResult.CandidateNode,
                            selectResult.DownInChord);
                        Console.WriteLine($"\t={rt}${selectResult.Tree.Name}@{selectResult.Tree.CurrentNode}");
                        if (rt == KeyProcessState.Continue)
                        {
                        }
                        else if (rt == KeyProcessState.Done)
                        {
                            selectTrees.Remove(selectResult);
                        }
                        else if (rt == KeyProcessState.NoFurtherProcess)
                        {
                            selectTrees.Remove(selectResult);
                            goto @return;
                        }
                        else if (rt == KeyProcessState.Reprocess || rt == KeyProcessState.Yield)
                        {
                            selectTrees.Remove(selectResult);
                            reprocess = true;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                } while (selectTrees.Count == 0 && /*no KeyProcessState.Continue*/
                         reprocess              && hasSelectedNodes /*Yield or Reprocess*/);

                @return:
                _stateTrees.ForEach(t => t.MarkDoneIfYield());
            }

            _eventSource.KeyDown += (sender, args) =>
            {
                ClimbTree(KeyEvent.Down, args);
                _keyDownHandlers.ForEach(h => h?.Invoke(sender, args));
            };
            _eventSource.KeyUp += (sender, args) =>
            {
                ClimbTree(KeyEvent.Up, args);
                _keyUpHandlers.ForEach(h => h?.Invoke(sender, args));
            };
        }

        private List<KeyStateTree.SelectionResult> SelectTree(KeyEvent eventType, KeyEventArgsExt args)
        {
            var selectedNodes = new List<KeyStateTree.SelectionResult>();
            //all on root, find current trees
            foreach (var stateTree in _stateTrees)
            {
                Debug.Assert(stateTree.IsOnRoot);
                if (stateTree.ProcessState == KeyProcessState.Yield) continue;

                var selectionResult = stateTree.TrySelect(eventType, args);
                if (selectionResult.CandidateNode == null) continue;

                if (selectedNodes.Count                           == 0 ||
                    selectionResult.CandidateNode.Key.ChordLength == selectedNodes[0].CandidateNode.Key.ChordLength)
                {
                    selectedNodes.Add(selectionResult);
                }
                else if (selectionResult.CandidateNode.Key.ChordLength > selectedNodes[0].CandidateNode.Key.ChordLength)
                {
                    selectedNodes.Clear();
                    selectedNodes.Add(selectionResult);
                }
            }

            if (selectedNodes.Count > 0)
                Console.WriteLine(
                    $"ToClimb:{string.Join(",", selectedNodes.Select(t => $"${t.Tree.Name}_{t.CandidateNode}"))}");
            return selectedNodes;
        }
    }
}