using System;
using System.Collections.Generic;
using Metatool.Input;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.DataStructures
{
    public class Trie<TKey,TValue> : TrieNode<TKey,TValue>, ITrie<TKey,TValue> where TKey:ICombination where TValue: KeyEventCommand
    {
        public IEnumerable<TValue> Get(IList<TKey> query)
        {
            return Get(query, 0);
        }

        public void Add(IList<TKey> query, TValue value)
        {
            Add(query, 0, value);
        }

        public void Add(TKey key, TValue value)
        {
            Add(new List<TKey>{key},0, value);
        }

        public bool Remove(IList<TKey> query,Predicate<TValue> predicate = null)
        {
            var r = Remove(query, 0, predicate);
            CleanPath(query, 0);
            return r;
        }

        public void Clear()
        {
            this.Clear();
        }
    }
}
