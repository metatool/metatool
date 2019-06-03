using System;
using System.Collections.Generic;
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
        internal static IDictionary< Keys, Action<KeyEventArgsExt>> KeyDown = new Dictionary<Keys, Action<KeyEventArgsExt>>();
        internal static IDictionary< Keys, Action<KeyPressEventArgsExt>> KeyPress = new Dictionary<Keys, Action<KeyPressEventArgsExt>>();
        internal static IDictionary< Keys, Action<KeyEventArgsExt>> KeyUp = new Dictionary<Keys, Action<KeyEventArgsExt>>();
        internal static IDictionary<ICombination, Action<KeyEventArgsExt>> Combinations = new Dictionary<ICombination, Action<KeyEventArgsExt>>();
        internal static IDictionary<Sequence, Action<KeyEventArgsExt>> Sequences = new Dictionary<Sequence, Action<KeyEventArgsExt>>();
        private static readonly Trie<Key,Combination>  _trie = new Trie<Key,Combination>();
        private static readonly TrieWalker<Key, Combination> _trieWalker = new TrieWalker<Key,Combination>(_trie);

        public static void AddCombination(Combination combination)
        {
            _trie.Add(new List<Key>{combination.Key}, combination);
        }

        public static void Run()
        {
            if(Combinations.Count>0)
                CombinationExtensions.ProcessCombination(Hook.GlobalEvents(), Combinations);
            if (Sequences.Count > 0)
                Hook.GlobalEvents().ProcessSequence(Sequences);

            var source = Hook.GlobalEvents();
            _trieWalker.GoToRoot();
            source.KeyDown += (sender, args) =>
            {
                var key = new Key(args.KeyCode, KeyEventType.Down);
                var b = _trieWalker.TryGoToChild(key);
                if (!b)
                {
                    _trieWalker.GoToRoot();
                    return;
                }
                var state = KeyboardState.GetCurrent();
                var maxLength = 0;
                List<KeyAction> actions = null;
                foreach (var current in _trieWalker.CurrentValues)
                {
                    var matches = current.Chord.All(state.IsDown);
                    if (!matches) continue;
                    if (maxLength > current.ChordLength) continue;
                    maxLength = current.ChordLength;
                    actions   = current.Actions;
                }

                if (actions == null)
                {
                    _trieWalker.GoToRoot();
                    return;
                }

                if (actions.Count == 0) return;

                actions[1].Action?.Invoke(args);
                _trieWalker.GoToRoot();
            };
        }

        internal static IKeyEvents HotKey(string keys)
        {
            if (keys.Contains(','))
            {
                return Sequence.FromString(keys);
            }

            return Combination.FromString(keys);
        }
    }
}
