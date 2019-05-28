using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.InputHook;
using Metaseed.KeyboardHook;
using KeyCombinationExtensions = Metaseed.KeyboardHook.KeyCombinationExtensions;

namespace Metaseed.Input
{
    public class KeyboardHook
    {
        internal static IDictionary< (Keys,KeyEventType), Action<KeyEventArgsExt>> Keys = new Dictionary< (Keys,KeyEventType), Action<KeyEventArgsExt>>();
        internal static IDictionary< Metaseed.Input.MouseKeyHook.Combination, Action<KeyEventArgsExt>> Combinations = new Dictionary< Metaseed.Input.MouseKeyHook.Combination, Action<KeyEventArgsExt>>();
        internal static IDictionary< Metaseed.Input.MouseKeyHook.Sequence, Action<KeyEventArgsExt>> Sequences = new Dictionary< Metaseed.Input.MouseKeyHook.Sequence, Action<KeyEventArgsExt>>();

        public static void Run()
        {
            if(Combinations.Count>0)
                KeyCombinationExtensions.ProcessCombination(Hook.GlobalEvents(), Combinations);
            if (Sequences.Count > 0)
                Hook.GlobalEvents().ProcessSequence(Sequences);
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
