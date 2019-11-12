using System.Collections.Generic;
using System.Windows.Forms;

namespace Metatool.Service
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

    }
}
