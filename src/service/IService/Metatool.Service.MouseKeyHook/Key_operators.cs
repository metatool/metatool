using System;
using System.Linq;
using System.Windows.Forms;

namespace Metatool.Service
{
    public partial class Key
    {

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
        public static Combination operator +(Key keyA, Key keyB)
        {
            return new Combination(keyB, keyA);
        }

        public ICombination ToCombination()
        {
            return (Combination) this;
        }

        public static bool operator ==(Key keyA, Key keyB)
        {
            return object.Equals(keyB, keyA);
        }

        public static bool operator !=(Key keyA, Key keyB)
        {
            return !Equals(keyB, keyA);
        }

        public static bool operator ==(Key keyA, Keys keyB)
        {
            if (keyA == null) return false;
            return keyA.Equals(keyB);
        }

        public static bool operator !=(Key keyA, Keys keyB)
        {
            if (keyA == null) return true;
            return !keyA.Equals(keyB);
        }
        public static bool operator ==(Keys keyA, Key keyB)
        {
            if (keyB == null) return false;

            return keyB.Equals(keyA);
        }

        public static bool operator !=(Keys keyA, Key keyB)
        {
            if (keyB == null) return true;
            return !keyB.Equals(keyA);
        }
        public static Key operator |(Key keyA, Key keyB)
        {
            return new Key(keyA.Codes.Concat(keyB.Codes).ToArray());
        }


        public bool IsEquals(Key key)
        {
            return Codes.IsSupersetOf(key.Codes);
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
        public int CompareTo(object obj)
        {
            return CompareTo((Key)obj);
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
            var hash = 0;
            var hc   = Codes.Count;
            foreach (var t in Codes)
            {
                hc = (int)unchecked(hc * 314159 + t.GetHashCode());
            }

            unchecked
            {
                hash = (hc + 13) ^
                       ((hc!= 0 ? (int)hc : 0) * 397);
            }

            return hash;
        }
    }
}
