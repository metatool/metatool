using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace Metaseed.Input
{
      /// <summary>
    /// https://www.w3.org/TR/uievents-key/
    /// </summary>
    public partial class Key : IComparable, IComparable<Key>, ISequencable
    {
        private SortedSet<Keys> _codes;
        private int             _val;
        public SortedSet<Keys> Codes
        {
            get => _codes;
            private set
            {
                _codes = value;
                _val   = value.Aggregate<Keys, int>(0, (o, c1) => o + (int) c1);
            }
        }

        public Key(Keys keyCode)
        {
            Codes = new SortedSet<Keys>() {keyCode};
        }

        public Key(Keys keyCode1, Keys keyCode2)
        {
            Codes = new SortedSet<Keys>() {keyCode1, keyCode2};
        }

        public Key(IEnumerable<Keys> keys)
        {
            Codes = new SortedSet<Keys>(keys);
        }

        public Key(Keys keyCode1, Keys keyCode2, Keys keyCode3)
        {
            Codes = new SortedSet<Keys>() {keyCode1, keyCode2, keyCode3};
        }

        public static Combination operator +(Key keyA, Key keyB)
        {
            return new Combination(keyB, keyA);
        }

        public static implicit operator Key(Keys keys)
        {
            return new Key(keys);
        }
        public static implicit operator Combination(Key key)
        {
            return new Combination(key);

        }
        public static explicit operator Keys(Key key)
        {
            return key.Codes.First();
        }

        public static bool operator ==(Key keyA, Key keyB)
        {
            return object.Equals(keyB, keyA);
        }

        public static bool operator !=(Key keyA, Key keyB)
        {
            return !object.Equals(keyB, keyA);
        }

        public bool IsEquals(Key key)
        {
            return Codes.Equals(key.Codes);
        }

        public bool Equals(Keys key)
        {
            return Codes.Contains(key);
        }

        public bool Equals(Key obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return IsEquals(obj);
        }


        public int CompareTo(Key other)
        {
            return _val - other._val;
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            switch (obj)
            {
                case Key k:
                    return IsEquals(k);
                case Keys keys:
                    return Equals(keys);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Codes.GetHashCode();
        }

        public override string ToString()
        {
            return $"({string.Join("|", Codes)})";
        }

        public int CompareTo(object obj)
        {
            return CompareTo((Key) obj);
        }
    }
}