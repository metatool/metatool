using System.Collections.Generic;
using System.Linq;

namespace Metatool.Service.MouseKey;

public partial class Combination
{
	public ICombination With(KeyCodes key)
	{
		return new Combination(TriggerKey, Chord.Concat(Enumerable.Repeat((Key)key, 1)));
	}

	public ICombination With(IEnumerable<KeyCodes> chordKeys)
	{
		return chordKeys.Aggregate(this as ICombination, (c, k) => c.With(k));
	}

	public ICombination Control()
	{
		return With(KeyCodes.Control);
	}

	public ICombination Alt()
	{
		return With(KeyCodes.Alt);
	}

	public ICombination Shift()
	{
		return With(KeyCodes.Shift);
	}
}