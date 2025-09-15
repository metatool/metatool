using System.Collections.Generic;
using System.Linq;

namespace Metatool.Service;

public partial class Combination
{
	public ICombination With(KeyValues key)
	{
		return new Combination(TriggerKey, Chord.Concat(Enumerable.Repeat((Key)key, 1)));
	}

	public ICombination With(IEnumerable<KeyValues> chordKeys)
	{
		return chordKeys.Aggregate(this as ICombination, (c, k) => c.With(k));
	}

	public ICombination Control()
	{
		return With(KeyValues.Control);
	}

	public ICombination Alt()
	{
		return With(KeyValues.Alt);
	}

	public ICombination Shift()
	{
		return With(KeyValues.Shift);
	}
}