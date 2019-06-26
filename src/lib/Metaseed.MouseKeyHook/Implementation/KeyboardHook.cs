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
        private readonly Trie<ICombination, KeyEventAction> _trie = new Trie<ICombination, KeyEventAction>();
        private readonly TrieWalker<ICombination, KeyEventAction> _trieWalker;
        private readonly IKeyboardMouseEvents _eventSource;

        public KeyboardHook()
        {
            _trieWalker = new TrieWalker<ICombination, KeyEventAction>(_trie);
            _eventSource = Hook.GlobalEvents();

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

        public class CombinationRemoveToken : IRemovable
        {
            private readonly ITrie<ICombination, KeyEventAction> _trie;
            private readonly IList<ICombination> _combinations;
            private readonly KeyEventAction _action;

            public CombinationRemoveToken(ITrie<ICombination, KeyEventAction> trie, IList<ICombination> combinations,
                KeyEventAction action)
            {
                _trie = trie;
                _combinations = combinations;
                _action = action;
            }

            public void Remove()
            {
                var r = _trie.Remove(_combinations, action => action.Equals(_action));
                Console.WriteLine(r);
            }
        }

        public IRemovable Add(IList<ICombination> combination, KeyEventAction action)
        {
            _trie.Add(combination, action);
            return new CombinationRemoveToken(_trie, combination, action);
        }

        public IRemovable Add(ICombination combination, KeyEventAction action)
        {
            return Add(new List<ICombination> {combination}, action);
        }

        public void Run()
        {
            _trieWalker.GoToRoot();

            void KeyEventProcess(KeyEvent eventType, KeyEventArgsExt args)
            {
                var downInChord = false;

                var child = _trieWalker.GetChildOrNull((ICombination acc, ICombination combination) =>
                {
                    if (eventType == KeyEvent.Down && combination.Chord.Contains(args.KeyCode)) downInChord = true;

                    if (args.KeyCode != combination.TriggerKey) return acc;
                    var mach = combination.Chord.All(args.KeyboardState.IsDown);
                    if (!mach) return acc;
                    if (acc == null) return combination;
                    return acc.ChordLength >= combination.ChordLength ? acc : combination;
                });

                // no match
                if (child == null)
                {
                    if (!downInChord && eventType == KeyEvent.Down && !_trieWalker.IsOnRoot)
                    {
                        _trieWalker.GoToRoot();
                        KeyEventProcess(eventType, args);
                    }

                    return; // waiting
                }

                // matched
                var actionList = child.Values() as KeyActionList<KeyEventAction>;
                Debug.Assert(actionList != null, nameof(actionList) + " != null");

                // execute
#if !DEBUG
                try
                {
#endif
                foreach (var keyEventAction in actionList[eventType])
                {
                    if (!string.IsNullOrEmpty(keyEventAction.Description))
                        Console.WriteLine(keyEventAction.Description);
                    keyEventAction.Action?.Invoke(args);
                }
#if !DEBUG
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
#endif

                // no children 
                if (_trieWalker.ChildrenCount == 0 && eventType == KeyEvent.Up)
                {
                    _trieWalker.GoToRoot();
                    return;
                }

                if (eventType == KeyEvent.Up)
                {
                    _trieWalker.GoToChild(child);
                   
                    if(child.ChildrenCount!=0) Notify.ShowKeysTip(child.Tip);
                    return;
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
            _eventSource.KeyDown += (sender, args) => KeyEventProcess(KeyEvent.Down, args as KeyEventArgsExt);
            _eventSource.KeyUp += (sender, args) => KeyEventProcess(KeyEvent.Up, args as KeyEventArgsExt);

            _keyDownHandlers.ForEach(h => _eventSource.KeyDown += h);
            _keyUpHandlers.ForEach(h => _eventSource.KeyUp += h);
        }

        public void ShowTip()
        {
            Notify.ShowKeysTip(_trieWalker.CurrentNode.Tip);
        }
    }
}