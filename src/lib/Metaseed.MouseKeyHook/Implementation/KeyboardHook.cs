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
        internal IDictionary<Keys, Action<KeyEventArgsExt>> KeyDown = new Dictionary<Keys, Action<KeyEventArgsExt>>();
        internal IDictionary<Keys, Action<KeyPressEventArgsExt>> KeyPress = new Dictionary<Keys, Action<KeyPressEventArgsExt>>();
        internal IDictionary<Keys, Action<KeyEventArgsExt>> KeyUp = new Dictionary<Keys, Action<KeyEventArgsExt>>();
        internal IDictionary<ICombination, Action<KeyEventArgsExt>> Combinations = new Dictionary<ICombination, Action<KeyEventArgsExt>>();
        internal IDictionary<Sequence, Action<KeyEventArgsExt>> Sequences = new Dictionary<Sequence, Action<KeyEventArgsExt>>();
        private readonly Trie<Combination, KeyAction> _trie = new Trie<Combination, KeyAction>();
        private readonly TrieWalker<Combination, KeyAction> _trieWalker = new TrieWalker<Combination, KeyAction>(_trie);

        public void AddCombination(IList<Combination> combination, KeyAction action)
        {
            _trie.Add(combination, action);
        }

        public void AddCombination(Combination combination, KeyAction action)
        {
            _trie.Add(new List<Combination>() { combination }, action);
        }

        public void Run()
        {
            var source = Hook.GlobalEvents();
            _trieWalker.GoToRoot();

            void keyEventProcess(KeyEventType eventType, KeyEventArgs args)
            {

                var state = KeyboardState.GetCurrent();
                var b = _trieWalker.TryGoToChild((acc, key) =>
                {
                    var mach = key.Chord.All(state.IsDown);
                    if (!mach) return acc;
                    if (acc == null) return key;
                    return acc.ChordLength >= key.ChordLength ? acc : key;
                });
                if (!b)
                {
                    _trieWalker.GoToRoot();
                    return;
                }
                // on child
                var actions = _trieWalker.CurrentValues as IList<KeyAction>;
                Debug.Assert(actions != null, nameof(actions) + " != null");

                // wait for next key
                if (actions.Count == 0) return;

                // execute and go to root
                actions[1].Action?.Invoke(args);
                _trieWalker.GoToRoot();
            }

            source.KeyDown += (sender, args) => keyEventProcess(KeyEventType.Down, args);
            source.KeyUp += (sender, args) => keyEventProcess(KeyEventType.Up, args);

        }

        internal IKeyEvents HotKey(string keys)
        {
            if (keys.Contains(','))
            {
                return Sequence.FromString(keys);
            }

            return Combination.FromString(keys);
        }
    }
}
