using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace Metaseed.DataStructures
{
    public class TrieNode<TKey, TValue> : TrieNodeBase<TKey, TValue>
    {
        private readonly Dictionary<TKey, TrieNode<TKey, TValue>> _children;
        private readonly List<TValue>                             _values;

        protected TrieNode()
        {
            _children = new Dictionary<TKey, TrieNode<TKey, TValue>>();
            _values   = new List<TValue>();
        }

        internal void Clear()
        {
            _children.Clear();
            _values.Clear();
        }


        protected override IEnumerable<TrieNodeBase<TKey, TValue>> Children()
        {
            return _children.Values;
        }

        protected internal override IEnumerable<TValue> Values()
        {
            return _values;
        }

        protected override bool IsRemovable(IList<TKey> query, int position)
        {
            return position < query.Count && _children.Count == 1 && _values.Count == 0 && _children.ContainsKey(query[position]) ||
                   position == query.Count  && _values.Count == 0 && _children.Count == 0;
        }

        protected override TrieNodeBase<TKey, TValue> GetOrCreateChild(TKey key)
        {
            if (_children.TryGetValue(key, out var result)) return result;
            result = new TrieNode<TKey, TValue>();
            _children.Add(key, result);
            return result;
        }

        internal TrieNode<TKey, TValue> GetChildOrNull( Func<TKey,TKey,TKey> aggregateFunc )
        {
            TKey key = _children.Keys.Aggregate(default(TKey),aggregateFunc);
            return GetChildOrNull(key);
        }
        internal TrieNode<TKey, TValue> GetChildOrNull(TKey key)
        {

            if (key == null) throw new ArgumentNullException(nameof(key));
            return _children.TryGetValue(key, out var childNode)
                    ? childNode
                    : null;
        }
        protected override TrieNodeBase<TKey, TValue> GetChildOrNull(IList<TKey> query, int position)
        {
            return GetChildOrNull(query[position]);
        }

        protected override void AddValue(TValue value)
        {
            _values.Add(value);
        }

        protected override bool RemoveValue(Predicate<TValue> predicate)
        {
            if (predicate == null)
            {
                _values.Clear();
                return true;
            }

            var i = _values.FindIndex(predicate);
            if (i == -1) return false;
            _values.RemoveAt(i);
            return true;
        }

        protected override void RemoveChild(TKey key)
        {
            _children.Remove(key);
        }
    }
}