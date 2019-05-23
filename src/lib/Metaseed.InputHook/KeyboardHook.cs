using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gma.System.MouseKeyHook;

namespace Metaseed.Input
{
    public class KeyboardHook
    {
        internal static IDictionary< Gma.System.MouseKeyHook.Combination, Action> Combinations = new Dictionary< Gma.System.MouseKeyHook.Combination, Action>();
        internal static IDictionary< Gma.System.MouseKeyHook.Sequence, Action> Sequences = new Dictionary< Gma.System.MouseKeyHook.Sequence, Action>();

        public static void Run()
        {
            if(Combinations.Count>0)
            Hook.GlobalEvents().OnCombination(Combinations);
            if(Sequences.Count>0)
            Hook.GlobalEvents().OnSequence(Sequences);
        }

        public static IKeyEvents Keys(string keys)
        {
            if (keys.Contains(','))
            {
                return Sequence.FromString(keys);
            }

            return Combination.FromString(keys);
        }
    }
}
