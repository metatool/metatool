using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;
using Metaseed.Input.MouseKeyHook.Implementation.Trie;

namespace Metaseed.Input.MouseKeyHook
{
    public class KeyboardHook
    {
        private readonly Trie<ICombination, KeyAction> _trie = new Trie<ICombination, KeyAction>();
        private readonly TrieWalker<ICombination, KeyAction> _trieWalker;
        public readonly IKeyboardMouseEvents EventSource;

        public KeyboardHook()
        {
            _trieWalker = new TrieWalker<ICombination, KeyAction>(_trie);
            EventSource = Hook.GlobalEvents();

        }

        public class CombinationRemoveToken: IDisposable
        {
            private readonly ITrie<ICombination, KeyAction> _trie;
            private readonly IList<ICombination> _combinations;
            private readonly KeyAction _action;

            public CombinationRemoveToken(ITrie<ICombination,KeyAction> trie,IList<ICombination> combinations, KeyAction action)
            {
                _trie = trie;
                _combinations = combinations;
                _action = action;
            }
            public void Dispose()
            {
                var r = _trie.Remove(_combinations, action => action == _action);
                Console.WriteLine(r);
            }
        }
        public IDisposable Add(IList<ICombination> combination, KeyAction action)
        {
            _trie.Add(combination, action);
            return new CombinationRemoveToken(_trie, combination, action);
        }

        public IDisposable Add(ICombination combination, KeyAction action)
        {
            return Add(new List<ICombination>{combination}, action);
        }

        public void Run()
        {
            _trieWalker.GoToRoot();
            Keys lastDownKeys = Keys.None;

            void KeyEventProcess(KeyEventType eventType, KeyEventArgsExt args)
            {
                // only process key UP and Press event on root
                if (eventType != KeyEventType.Down && !_trieWalker.IsOnRoot) return;

                var keyboardState = KeyboardState.GetCurrent();
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
                try
                {
                    actions.ForEach(a => a.Action?.Invoke(args));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

                // no children 
                if(_trieWalker.ChildrenCount == 0) _trieWalker.GoToRoot();
            }

            EventSource.KeyDown += (sender, args) =>
            {
                lastDownKeys = args.KeyCode;
                KeyEventProcess(KeyEventType.Down, args as KeyEventArgsExt);
            };
            EventSource.KeyUp += (sender, args) =>
            {
                KeyEventProcess(KeyEventType.Up, args as KeyEventArgsExt);
                if (args.KeyCode == lastDownKeys) KeyEventProcess(KeyEventType.Hit,args as KeyEventArgsExt);
            };

        }


    }
}
