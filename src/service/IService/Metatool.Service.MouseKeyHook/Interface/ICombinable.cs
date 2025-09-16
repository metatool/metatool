using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public interface ICombinable
{
	ICombination With(KeyValues chordKey);
	ICombination With(IEnumerable<KeyValues> keys);
	ICombination Control();
	ICombination Shift();
	ICombination Alt();
}