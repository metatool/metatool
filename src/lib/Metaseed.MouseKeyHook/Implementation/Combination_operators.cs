using System.Linq;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public partial class Combination
    {
        public static Combination operator +(Combination combA, ICombination combB)
        {
            return new Combination(combB.TriggerKey, combA.AllKeys.Concat(combB.Chord));
        }

        public static Combination operator +(Combination keyA, Key keyB)
        {
            return new Combination(keyB, keyA.AllKeys);
        }

        public static implicit operator Combination(Keys keys)
        {
            return new Combination(keys);
        }

        protected bool Equals(Combination other)
        {
            return
                TriggerKey == other.TriggerKey
                && Chord.Equals(other.Chord);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Combination)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Chord.GetHashCode() ^ TriggerKey.GetHashCode();
        }
    }
}