using System.Collections.Generic;
using System.Windows.Forms;
using Metatool.Command;

namespace Metatool.Input
{
    public static class KeysExtensions
    {
        public static ICombination With(this Key key, Keys chord)
        {
            return new Combination(key, chord);
        }

        public static ICombination With(this Key triggerKey, IEnumerable<Key> chordsKeys)
        {
            return new Combination(triggerKey, chordsKeys);
        }


        public static IKeyboardCommandToken  AsChordKey(this Key key)
        {
            return key.MapOnHit(key.ToCombination(), e => !e.IsVirtual , false);
        }
    }
}
