using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.Input.MouseKeyHook.Implementation.Trie;
using Metaseed.MetaKeyboard;

namespace Metaseed.Input.MouseKeyHook
{
    public class KeyboardHook
    {
        private readonly        IKeyboardMouseEvents _eventSource;
        private static readonly KeyStateMachine      DefaultStateMachine = new KeyStateMachine();
        KeyStateMachine                              _currentMachine;

        private readonly List<KeyStateMachine> _stateMachines = new List<KeyStateMachine>()
            {DefaultStateMachine};

        public KeyboardHook()
        {
            _eventSource = Hook.GlobalEvents();
        }

        public IRemovable Add(IList<ICombination> combinations, KeyEventAction action,
            KeyStateMachine keyStateMachine = null)
        {
            return (keyStateMachine ?? DefaultStateMachine).Add(combinations, action);
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

        public void ShowTip()
        {
            if (_currentMachine != null) Notify.ShowKeysTip(_currentMachine.Tips);
            var tips = _stateMachines.SelectMany(m => m.Tips);
            Notify.ShowKeysTip(tips);
        }


        public void Run()
        {
            _stateMachines.ForEach(m => m.Reset());


            void KeyEventProcess(KeyEvent eventType, KeyEventArgsExt args)
            {
                // continue process this event on current machine
                if (_currentMachine != null)
                {
                    var result = _currentMachine.KeyEventProcess(eventType, args);
                    if (result == KeyProcessState.Continue) return;
                    else if (result == KeyProcessState.Done)
                    {
                        _currentMachine = null;
                        return; // try to find current machine on next event 
                    }

                    else if (result == KeyProcessState.Reset)
                        _currentMachine = null;

                    // yield: try to process this event with other machine.
                }

                // find current machine
                foreach (var keyStateMachine in _stateMachines)
                {
                    if(keyStateMachine == _currentMachine) continue; // yield

                    var result = keyStateMachine.KeyEventProcess(eventType, args);
                    if (result == KeyProcessState.Continue)
                    {
                        _currentMachine = keyStateMachine;
                        break;
                    }
                    else if (result == KeyProcessState.Done)
                    {
                        break; // this event is well handled
                    }

                    // yield or reset: continue finding
                }
            }

            _eventSource.KeyUp += (o, e) =>
            {
                if (KeyboardState.HandledDownKeys.Contains(e.KeyCode))
                {
                    KeyboardState.HandledDownKeys.Remove(e.KeyCode);
                    e.Handled = true;
                }
            };
            _eventSource.KeyDown += (sender, args) =>
                KeyEventProcess(KeyEvent.Down, args as KeyEventArgsExt);
            _eventSource.KeyUp += (sender, args) =>
                KeyEventProcess(KeyEvent.Up, args as KeyEventArgsExt);

            _keyDownHandlers.ForEach(h => _eventSource.KeyDown += h);
            _keyUpHandlers.ForEach(h => _eventSource.KeyUp += h);
        }
    }
}