using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metatool.Service.MouseKey;

public class Chord : IEnumerable<Key>
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
            .Select(Enum.Parse<KeyValues>);

        return new Chord(parts);
    }

    protected bool Equals(Chord other)
    {
        if (_keys.Length != other._keys.Length) 
            return false;

        return _keys.SequenceEqual(other._keys);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) 
            return false;

        if (ReferenceEquals(this, obj)) 
            return true;

        if (obj.GetType() != GetType()) 
            return false;

        return Equals((Chord)obj);
    }

    public override int GetHashCode()
    {
        var hash = 0;
        var hc = _keys.Length;
        foreach (var t in _keys)
        {
            hc = unchecked(hc * 314159 + t.GetHashCode());
        }

        unchecked
        {
            hash = hc + 13 ^
                   (hc != 0 ? hc : 0) * 397;
        }

        return hash;
    }
}