using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ICombination 
    {
        Keys TriggerKey { get; }
        IEnumerable<Keys> Chord { get; }
        int ChordLength { get; }

        ICombination With(Keys chordKey);
        ICombination With(IEnumerable<Keys> keys);
        ICombination Control();
        ICombination Shift();
        ICombination Alt();

        ISequence Then(Keys key);
        ISequence Then(ICombination combination);

        bool IsAnyKey(Keys key);

    }
}