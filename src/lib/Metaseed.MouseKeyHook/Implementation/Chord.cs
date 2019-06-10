// This code is distributed under MIT license. 
// Copyright (c) 2010-2018 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    internal class Chord : IEnumerable<Keys>
    {
        private readonly Keys[] _keys;

        internal Chord(IEnumerable<Keys> additionalKeys)
        {
            _keys = additionalKeys.OrderBy(k => k).ToArray();
        }

        public int Count
        {
            get { return _keys.Length; }
        }

        public IEnumerator<Keys> GetEnumerator()
        {
            return _keys.Cast<Keys>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join("+", _keys);
        }

        public static Chord FromString(string chord)
        {
            var parts = chord
                .Split('+')
                .Select(p => Enum.Parse(typeof(Keys), p))
                .Cast<Keys>();
            var stack = new Stack<Keys>(parts);
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

        private int _hash = 0;
        public override int GetHashCode()
        {
            if (_hash != 0) return _hash;
            var hc = _keys.Length;
            foreach (var t in _keys)
            {
                hc = (int) unchecked(hc * 314159 + t);
            }
            unchecked
            {
              _hash = (_keys.Length + 13) ^
                       ((_keys.Length != 0 ? (int) hc : 0) * 397);
            }

            return _hash;
        }
    }
}