using System.Collections.Generic;
using System.Linq;

namespace Metatool.Service.MouseKey;

/// <summary>
///     Describes a sequence of generic objects.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SequenceList<T> :List<T>, IEnumerable<T>
{
	protected SequenceList(params T[] elements)
	{
		AddRange(elements);
	}

	public override string ToString()
	{
		return string.Join(", ", this);
	}

	protected bool Equals(SequenceList<T> other)
	{
		if (Count != other.Count) return false;
		return this.SequenceEqual(other);
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((SequenceList<T>) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (Count + 13) ^
			       ((Count != 0
				       ? this[0].GetHashCode() ^ this[^1].GetHashCode()
				       : 0) * 397);
		}
	}
}