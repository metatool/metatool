using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.UI;
using Microsoft.Extensions.Logging;

namespace Metatool.Input.MouseKeyHook
{
    public delegate void KeyEventHandler(object sender, IKeyEventArgs e);

    public class KeyboardHook
    {
        public           INotify               Notify { get; }
        private readonly ILogger<KeyboardHook> _logger;
        private readonly IKeyboardMouseEvents  _eventSource;
        public           bool                  IsRuning { get; set; }


        public KeyboardHook(ILogger<KeyboardHook> logger, INotify notify)
        {
            Notify              = notify;
            KeyStateTree.Notify = notify;
            _logger             = logger;
            _eventSource        = Hook.GlobalEvents();
        }

        public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command,
            string stateTree = KeyStateTrees.Default)
        {
            var keyStateTree = KeyStateTree.GetOrCreateStateTree(stateTree);
            return keyStateTree.Add(combinations, command);
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
            var tips = KeyStateTree.StateTrees.Values.SelectMany(m => m.Tips(ifRootThenEmpty)).ToArray();
            if (tips.Length > 0)
                Notify?.ShowKeysTip(tips);
            else
            {
                Notify?.CloseKeysTip();
            }
        }

        private const int MaxRecursiveCount = 50;

        public void Run()
        {
            if (IsRuning) return;
            var access = System.Windows.Application.Current.Dispatcher.CheckAccess();
            if (!access)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) Run);
                return;
            }

            IsRuning = true;
            _logger.LogInformation($"Keyboard hook is running...");

            foreach (var stateTree in KeyStateTree.StateTrees.Values) stateTree.Reset();

            var selectTrees = new List<KeyStateTree.SelectionResult>();

            //eventType is only Down or Up
            void ClimbTree(KeyEvent eventType, IKeyEventArgs args)
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
                        var selectResult = c; // should not remove this line
                        if (!onGround)
                        {
                            var r   = selectResult.Tree.TrySelect(eventType, args);
                            var ind = selectTrees.IndexOf(selectResult);
                            selectTrees[ind] = r;
                            selectResult     = r;
                        }

                        var rt = selectResult.Tree.Climb(eventType, args, selectResult.CandidateNode,
                            selectResult.DownInChord);
                        _logger.LogInformation($"\t={rt}${selectResult.Tree.Name}@{selectResult.Tree.CurrentNode}");
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
                foreach (var stateTree in KeyStateTree.StateTrees.Values) stateTree.MarkDoneIfYield();
            }

            _eventSource.KeyDown += (sender, args) =>
            {
                ClimbTree(KeyEvent.Down, args);
                var handlers = new List<KeyEventHandler>(_keyDownHandlers); // a copy, so newly added would be handled in next event.
                handlers.ForEach(h => h?.Invoke(sender, args));
            };
            _eventSource.KeyUp += (sender, args) =>
            {
                ClimbTree(KeyEvent.Up, args);
                var handlers = new List<KeyEventHandler>(_keyDownHandlers); // a copy
                handlers.ForEach(h => h?.Invoke(sender, args));
            };
        }

        private List<KeyStateTree.SelectionResult> SelectTree(KeyEvent eventType, IKeyEventArgs args)
        {
            var selectedNodes = new List<KeyStateTree.SelectionResult>();
            //all on root, find current trees
            foreach (var stateTree in KeyStateTree.StateTrees.Values)
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
                _logger.LogInformation(
                    $"ToClimb:{string.Join(",", selectedNodes.Select(t => $"${t.Tree.Name}_{t.CandidateNode}"))}");
            return selectedNodes;
        }
    }
}