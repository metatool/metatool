using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gma.System.MouseKeyHook;
using Metaseed.KeyboardHook;

namespace Metaseed.Input
{
    public class KeyboardHook
    {
        internal static IDictionary< Gma.System.MouseKeyHook.Combination, Action<KeyEventArgsExt>> Combinations = new Dictionary< Gma.System.MouseKeyHook.Combination, Action<KeyEventArgsExt>>();
        internal static IDictionary< Gma.System.MouseKeyHook.Sequence, Action<KeyEventArgsExt>> Sequences = new Dictionary< Gma.System.MouseKeyHook.Sequence, Action<KeyEventArgsExt>>();

        public static void Run()
        {
            if(Combinations.Count>0)
            Hook.GlobalEvents().ProcessCombination(Combinations);
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
