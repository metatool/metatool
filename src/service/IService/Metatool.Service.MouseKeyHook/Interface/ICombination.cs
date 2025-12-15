using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public interface ICombination :IKeyPath, ISequencable, ICombinable, ISequenceUnit
{
	Key TriggerKey { get; }
	IEnumerable<Key> Chord { get; }
	int ChordCount { get; }
    /// <summary>
    /// chord keys and trigger key
    /// </summary>
    IEnumerable<Key> AllKeys { get; }

	bool IsAnyKey(KeyCodes key);
    //string Description { get; }
    // user friendly name, not as tostring which return the keyCode
    string KeyName { get; }
}