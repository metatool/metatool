using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metatool.Service.MouseKeyHook.Implementation;

internal class Chord : IEnumerable<Key>
{
	private readonly Key[] _keys;

	internal Chord(IEnumerable<Key> chordKeys)
	{
		_keys = chordKeys.OrderBy(k => k).ToArray();
	}

	internal Chord(IEnumerable<KeyValues> chordKeys) : this(chordKeys.Select(k => new Key(k)))
	{
	}

	public int Count => _keys.Length;

	public IEnumerator<Key> GetEnumerator()
	{
		return _keys.Cast<Key>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		return string.Join("+", _keys as IEnumerable<Key>);
	}

	public static Chord FromString(string chord)
	{
		var parts = chord
			.Split('+')
			.Select(p => Enum.Parse(typeof(KeyValues), p))
			.Cast<KeyValues>();
		var stack = new Stack<KeyValues>(parts);
		return new Chord(stack);
	}

	protected bool Equals(Chord other)
	{
		if (_keys.Length != other._keys.Length) return false;
		return _keys.SequenceEqual(other._keys);
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Chord) obj);
	}

	public override int GetHashCode()
	{
		var hash = 0;
		var hc   = _keys.Length;
		foreach (var t in _keys)
		{
			hc = (int) unchecked(hc * 314159 + t.GetHashCode());
		}

		unchecked
		{
			hash = (hc + 13) ^
			       ((hc != 0 ? (int) hc : 0) * 397);
		}

		return hash;
	}
}