using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public interface ICombination :IKeyPath, ISequencable, ICombinable, ISequenceUnit
{
	Key TriggerKey { get; }
	IEnumerable<Key> Chord { get; }
	int ChordLength { get; }
	IEnumerable<Key> AllKeys { get; }

	bool IsAnyKey(KeyValues key);
}