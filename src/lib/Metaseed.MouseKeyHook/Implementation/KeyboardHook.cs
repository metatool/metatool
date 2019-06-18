using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.Input.MouseKeyHook.Implementation.Trie;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

namespace Metaseed.Input.MouseKeyHook
{
    public class KeyboardHook
    {
        private readonly Trie<ICombination, KeyAction> _trie = new Trie<ICombination, KeyAction>();
        private readonly TrieWalker<ICombination, KeyAction> _trieWalker;
        private readonly IKeyboardMouseEvents _eventSource;

        public KeyboardHook()
        {
            _trieWalker = new TrieWalker<ICombination, KeyAction>(_trie);
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
            private readonly ITrie<ICombination, KeyAction> _trie;
            private readonly IList<ICombination> _combinations;
            private readonly KeyAction _action;

            public CombinationRemoveToken(ITrie<ICombination, KeyAction> trie, IList<ICombination> combinations, KeyAction action)
            {
                _trie = trie;
                _combinations = combinations;
                _action = action;
            }
            public void Remove()
            {
                var r = _trie.Remove(_combinations, action => action == _action);
                Console.WriteLine(r);
            }
        }
        public IRemovable Add(IList<ICombination> combination, KeyAction action)
        {
            _trie.Add(combination, action);
            return new CombinationRemoveToken(_trie, combination, action);
        }

        public IRemovable Add(ICombination combination, KeyAction action)
        {
            return Add(new List<ICombination> { combination }, action);
        }

        public void Run()
        {
            _trieWalker.GoToRoot();

            void KeyEventProcess(KeyEventType eventType, KeyEventArgsExt args)
            {
                // only process key UP and Press event on root
                if (eventType != KeyEventType.Down && !_trieWalker.IsOnRoot) return;

                var keyboardState = args.KeyboardState;
                var success = _trieWalker.TryGoToChild((acc, key) =>
                {
                    if (args.KeyCode != key.TriggerKey || eventType != key.EventType) return acc;
                    var mach = key.Chord.All(keyboardState.IsDown);
                    if (!mach) return acc;
                    if (acc == null) return key;
                    return acc.ChordLength >= key.ChordLength ? acc : key;
                });

                // no match, to root
                if (!success)
                {
                    _trieWalker.GoToRoot();
                    return;
                }

                // on matched child
                var actions = _trieWalker.CurrentValues as List<KeyAction>;
                Debug.Assert(actions != null, nameof(actions) + " != null");

                // execute
#if !DEBUG
                try
                {
#endif
                actions.ForEach(a =>
                    {
                        if (!string.IsNullOrEmpty(a.Description)) Console.WriteLine(a.Description);
                        a.Action?.Invoke(args);
                    });
#if !DEBUG
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
#endif
                // no children 
                if (_trieWalker.ChildrenCount == 0) _trieWalker.GoToRoot();
            }

            _eventSource.KeyDown += (sender, args) => KeyEventProcess(KeyEventType.Down, args as KeyEventArgsExt);
            _eventSource.KeyUp += (sender, args) => KeyEventProcess(KeyEventType.Up, args as KeyEventArgsExt);

            _keyDownHandlers.ForEach(h => _eventSource.KeyDown += h);
            _keyUpHandlers.ForEach(h => _eventSource.KeyUp += h);


        }


    }
}
