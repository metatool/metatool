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
        private readonly Trie<Combination, KeyAction> _trie = new Trie<Combination, KeyAction>();
        private readonly TrieWalker<Combination, KeyAction> _trieWalker;

        public KeyboardHook()
        {
            _trieWalker = new TrieWalker<Combination, KeyAction>(_trie);
        }

        public void Add(IList<ICombination> combination, KeyAction action)
        {
            _trie.Add(new List<Combination>(combination.Cast<Combination>()), action);
        }

        public void Add(ICombination combination, KeyAction action)
        {
            _trie.Add(new List<Combination>() { combination as Combination }, action);
        }

        public void Run()
        {
            var source = Hook.GlobalEvents();
            _trieWalker.GoToRoot();

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
                actions.ForEach(a => a.Action?.Invoke(args));

                // no children 
                if(_trieWalker.ChildrenCount == 0) _trieWalker.GoToRoot();
            }

            source.KeyDown += (sender, args) => KeyEventProcess(KeyEventType.Down, args as KeyEventArgsExt);
            source.KeyUp += (sender, args) => KeyEventProcess(KeyEventType.Up, args as KeyEventArgsExt);

        }


    }
}
