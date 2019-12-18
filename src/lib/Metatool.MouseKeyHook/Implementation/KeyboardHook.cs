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

    public delegate void KeyPressEventHandler(object sender, IKeyPressEventArgs e);

    public class KeyboardHook
    {
        public           INotify               Notify { get; }
        private readonly ILogger<KeyboardHook> _logger;
        private readonly IKeyboardMouseEvents  _eventSource;
        public           bool                  IsRuning { get; set; }

        public bool Disable
        {
            get => _eventSource.Disable;
            set => _eventSource.Disable = value;
        }

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

            //eventType is only Down or Up
            static void ClimbTree(KeyEvent eventType, IKeyEventArgs args, ILogger logger) // should be static to be as a reentrancy method
            {
                var selectedTrees = new List<KeyStateTree.SelectionResult>();

                // if machine_1 has A+B and machine_2's A and B, press A+B on machine_1 would be processed
                // if machine_1 has A and machine_2 has A, both should be processed.
                // runs like all are in the same tree, but provide state jump for every tree
                // // continue process this event on current machine
                bool reprocess;
                do
                {
                    reprocess = false;
                    var onGround = false;
                    if (selectedTrees.Count == 0)
                    {
                        onGround      = true;
                        selectedTrees = SelectTree(eventType, args, logger);
                    }

                    var hasSelectedNodes = selectedTrees.Count > 0;
                    if (!hasSelectedNodes) goto @return;

                    var trees = selectedTrees.GetRange(0, selectedTrees.Count);
                    foreach (var c in trees)
                    {
                        var selectedTree = c; // should not remove this line
                        if (!onGround)
                        {
                            var result = selectedTree.Tree.TrySelect(eventType, args);
                            var index  = selectedTrees.IndexOf(selectedTree);
                            selectedTrees[index] = result;
                            selectedTree         = result;
                        }

                        var rt = selectedTree.Tree.Climb(eventType, args, selectedTree.CandidateNode,
                            selectedTree.DownInChord);
                        logger.LogInformation($"\t={rt}${selectedTree.Tree.Name}@{selectedTree.Tree.CurrentNode}");
                        if (rt == KeyProcessState.Continue)
                        {
                        }
                        else if (rt == KeyProcessState.Done)
                        {
                            selectedTrees.Remove(selectedTree);
                        }
                        else if (rt == KeyProcessState.NoFurtherProcess)
                        {
                            selectedTrees.Remove(selectedTree);
                            goto @return;
                        }
                        else if (rt == KeyProcessState.Reprocess || rt == KeyProcessState.Yield)
                        {
                            selectedTrees.Remove(selectedTree);
                            reprocess = true;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                } while (selectedTrees.Count == 0 && /*no KeyProcessState.Continue*/
                         reprocess /*Yield or Reprocess*/);

                @return:
                foreach (var stateTree in KeyStateTree.StateTrees.Values) stateTree.MarkDoneIfYield();
            }

            _eventSource.KeyDown += (sender, args) =>
            {
                ClimbTree(KeyEvent.Down, args, _logger);
                var handlers =
                    new List<KeyEventHandler>(
                        _keyDownHandlers); // a copy, so newly added would be handled in next event.
                handlers.ForEach(h => h?.Invoke(sender, args));
            };
            _eventSource.KeyUp += (sender, args) =>
            {
                ClimbTree(KeyEvent.Up, args, _logger);
                var handlers = new List<KeyEventHandler>(_keyUpHandlers); // a copy
                handlers.ForEach(h => h?.Invoke(sender, args));
            };
        }

        static List<KeyStateTree.SelectionResult> SelectTree(KeyEvent eventType, IKeyEventArgs args, ILogger logger)
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
                logger.LogInformation(
                    $"ToClimb:{string.Join(",", selectedNodes.Select(t => $"${t.Tree.Name}_{t.CandidateNode}"))}");
            return selectedNodes;
        }
    }
}