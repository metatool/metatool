using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metatool.Service
{
    /// <summary>
    ///     Describes a sequence of generic objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SequenceBase<T> : IEnumerable<T>
    {
        private readonly T[] _elements;

        protected SequenceBase(params T[] elements)
        {
            _elements = elements;
        }

        public int Length => _elements.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return _elements.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(",", _elements);
        }

        protected bool Equals(SequenceBase<T> other)
        {
            if (_elements.Length != other._elements.Length) return false;
            return _elements.SequenceEqual(other._elements);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SequenceBase<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_elements.Length + 13) ^
                       ((_elements.Length != 0
                            ? _elements[0].GetHashCode() ^ _elements[^1].GetHashCode()
                            : 0) * 397);
            }
        }
    }
}
