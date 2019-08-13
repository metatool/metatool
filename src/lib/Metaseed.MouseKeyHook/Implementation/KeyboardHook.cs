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
            int recursiveCount = 0;

          
            KeyStateTree currentTree = null;


            void KeyEventProcess(KeyEvent eventType, KeyEventArgsExt args)
            {
                //Q: what if key sent in other machine, and we are on the keypath
                //A: we could use is virtual key filter
                // foreach (var keyStateMachine in _stateMachines)
                // {
                //     var result = keyStateMachine.KeyEventProcess(eventType, args);
                //     if (result == KeyProcessState.NoFurtherProcess)
                //         return;
                // }

                // if machine_1 has A+B and machine_2's A+B would never processed
                // continue process this event on current machine


                if (currentTree != null)
                {
                    var result = currentTree.ProcessKeyEvent(eventType, args);
                    switch (result)
                    {
                        case KeyProcessState.Continue:
                            return;
                        case KeyProcessState.Done:
                            currentTree = null;
                            return; // try to find current tree on next event 
                        case KeyProcessState.Reprocess:
                            currentTree = null;
                            break;
                        case KeyProcessState.Yield:
                            break;
                        case KeyProcessState.NoFurtherProcess:
                            currentTree = null;
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // yield: try to process this event with other machine.
                }

                reprocess:
                if (recursiveCount > 1) Console.WriteLine("&"); // trace recurrent

                if (recursiveCount > MaxRecursiveCount)
                {
                    Console.WriteLine($"\tRecursiveCount: {recursiveCount}>{MaxRecursiveCount}");
                    recursiveCount = 0;
                }

                // find current tree
                foreach (var stateTree in _stateMachines)
                {
                    if (stateTree == currentTree) continue; // yield

                    var result = stateTree.ProcessKeyEvent(eventType, args);
                    if (result == KeyProcessState.Reprocess) recursiveCount++;
                    else recursiveCount--;
                    switch (result)
                    {
                        case KeyProcessState.Continue:
                            currentTree = stateTree;
                            return;
                        case KeyProcessState.Done:
                            currentTree = null;
                            return; // try to find current tree on next event 
                        case KeyProcessState.Reprocess:

                            currentTree = null;
                            goto reprocess;

                        case KeyProcessState.Yield:
                            break;
                        case KeyProcessState.NoFurtherProcess:
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            _eventSource.KeyDown += (sender, args) =>
                KeyEventProcess(KeyEvent.Down, args as KeyEventArgsExt);
            _eventSource.KeyUp += (sender, args) =>
                KeyEventProcess(KeyEvent.Up, args as KeyEventArgsExt);

            _keyDownHandlers.ForEach(h => _eventSource.KeyDown += h);
            _keyUpHandlers.ForEach(h => _eventSource.KeyUp     += h);
        }
    }
}