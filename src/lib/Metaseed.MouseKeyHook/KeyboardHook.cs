using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook;
using Metaseed.InputHook;
using Metaseed.KeyboardHook;
using KeyCombinationExtensions = Metaseed.KeyboardHook.KeyCombinationExtensions;

namespace Metaseed.Input
{
    public class KeyboardHook
    {
        internal static IDictionary< Keys, Action<KeyEventArgsExt>> KeyDown = new Dictionary<Keys, Action<KeyEventArgsExt>>();
        internal static IDictionary< Keys, Action<KeyPressEventArgsExt>> KeyPress = new Dictionary<Keys, Action<KeyPressEventArgsExt>>();
        internal static IDictionary< Keys, Action<KeyEventArgsExt>> KeyUp = new Dictionary<Keys, Action<KeyEventArgsExt>>();
        internal static IDictionary< Metaseed.Input.MouseKeyHook.KeysCombination, Action<KeyEventArgsExt>> Combinations = new Dictionary< Metaseed.Input.MouseKeyHook.KeysCombination, Action<KeyEventArgsExt>>();
        internal static IDictionary< Metaseed.Input.MouseKeyHook.Sequence, Action<KeyEventArgsExt>> Sequences = new Dictionary< Metaseed.Input.MouseKeyHook.Sequence, Action<KeyEventArgsExt>>();
        private static ITrie<ICombination,Action<EventArgs>>  _trie = new Trie<ICombination, Action<EventArgs>>();

        public static void Run()
        {
            if(Combinations.Count>0)
                CombinationExtensions.ProcessCombination(Hook.GlobalEvents(), Combinations);
            if (Sequences.Count > 0)
                Hook.GlobalEvents().ProcessSequence(Sequences);

            _trie.Remove()
            var source = Hook.GlobalEvents();
            source.KeyDown += (sender, args) =>
            {
                var combination = new Combination(e.KeyCode);

            };
        }

        internal static IKeyEvents Shortcut(string keys)
        {
            if (keys.Contains(','))
            {
                return Sequence.FromString(keys);
            }

            return Combination.FromString(keys);
        }
    }
}
