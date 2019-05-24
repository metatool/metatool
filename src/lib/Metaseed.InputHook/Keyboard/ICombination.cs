using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ICombination : IKeyEvents
    {
        ICombination With(Keys chordKey);
        ICombination With(IEnumerable<Keys> keys);
        ISequence Then(Keys key);
        ISequence Then(ICombination combination);
    }
}