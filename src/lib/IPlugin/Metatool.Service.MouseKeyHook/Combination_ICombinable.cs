using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Metatool.Input
{
    public partial class Combination
    {
        public ICombination With(Keys key)
        {
            return new Combination(TriggerKey, Chord.Concat(Enumerable.Repeat((Key) key, 1)));
        }

        public ICombination With(IEnumerable<Keys> chordKeys)
        {
            return chordKeys.Aggregate(this as ICombination, (c, k) => c.With(k));
        }

        public ICombination Control()
        {
            return With(Keys.Control);
        }

        public ICombination Alt()
        {
            return With(Keys.Alt);
        }

        public ICombination Shift()
        {
            return With(Keys.Shift);
        }
    }
}
