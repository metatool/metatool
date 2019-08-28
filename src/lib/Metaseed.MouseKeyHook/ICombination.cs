using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{

    public interface ISequenceUnit: ISequencable
    {
        ICombination ToCombination();
    }
    public interface ICombination :IKeyPath, ISequencable, ICombinable, ISequenceUnit
    {
        Key TriggerKey { get; }
        IEnumerable<Key> Chord { get; }
        int ChordLength { get; }
        IEnumerable<Key> AllKeys { get; }

        bool IsAnyKey(Keys key);

    }
}