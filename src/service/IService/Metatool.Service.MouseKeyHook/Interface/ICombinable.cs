using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public interface ICombinable
{
	ICombination With(KeyCodes chordKey);
	ICombination With(IEnumerable<KeyCodes> keys);
	ICombination Control();
	ICombination Shift();
	ICombination Alt();
}