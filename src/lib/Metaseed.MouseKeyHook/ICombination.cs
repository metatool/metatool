using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ICombination : IKeyEvents
    {
        Keys TriggerKey { get; }
        IEnumerable<Keys> Chord { get; }
        int ChordLengh { get; }

        KeyEventType EventType;
        ICombination With(Keys chordKey);
        ICombination With(IEnumerable<Keys> keys);
        ICombination Control();
        ICombination Shift();
        ICombination Alt();

        ISequence Then(Keys key);
        ISequence Then(ICombination combination);
    }
}