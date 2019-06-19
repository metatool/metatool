using System;
using System.Collections.Generic;
using System.Linq;
using Metaseed.DataStructures;

namespace Metaseed.Input.MouseKeyHook.Implementation.Trie
{
    public class TrieWalker<TKey,TValue> where TKey:ICombination where TValue: KeyEventAction
    {
        private readonly Trie<TKey, TValue> _trie;

        public TrieWalker(Trie<TKey, TValue> trie)
        {
            _trie = trie;
            CurrentNode = _trie;
        }

        private TrieNode<TKey, TValue> CurrentNode { get; set; }
        public bool IsOnRoot => CurrentNode == _trie;

        public int ChildrenCount => CurrentNode.ChildrenCount;
        public IEnumerable<TValue> CurrentValues => CurrentNode.Values();

        public bool TryGoToChild(TKey key)
        {
            var node = CurrentNode.GetChildOrNull(key);
            if (node == null) return false;
            CurrentNode = node;
            return true;
        }
        public bool TryGoToChild(Func<TKey, TKey, TKey> aggregateFunc, TKey initialKey = default(TKey))
        {
            var node = CurrentNode.GetChildOrNull(aggregateFunc, initialKey);
            if (node == null) return false;
            CurrentNode = node;
            return true;
        }
        internal TrieNode<TKey, TValue> GetChildOrNull(Func<TKey, TKey, TKey> aggregateFunc, TKey initialKey = default(TKey))
        {
            return CurrentNode.GetChildOrNull(aggregateFunc, initialKey);

        }

        internal KeyValuePair<TKey, TrieNode<TKey, TValue>> GetChildOrNull(Func<KeyValuePair<TKey, TrieNode<TKey, TValue>>, KeyValuePair<TKey,TrieNode<TKey, TValue>>, KeyValuePair<TKey, TrieNode<TKey, TValue>>> aggregateFunc, KeyValuePair<TKey, TrieNode<TKey, TValue>> initKey=default(KeyValuePair<TKey,TrieNode<TKey,TValue>>))
        {
            return CurrentNode.ChildrenPairs.Aggregate(initKey, aggregateFunc);
        }

        public void GoToChild(TrieNode<TKey, TValue> child)
        {
            CurrentNode = child;
        }

        public void GoToRoot()
        {
            CurrentNode = _trie;
        }
    }
}
