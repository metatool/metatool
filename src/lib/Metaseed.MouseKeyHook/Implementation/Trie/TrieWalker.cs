using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Metaseed.DataStructures;

namespace Metaseed.Input.MouseKeyHook.Implementation.Trie
{
    public class TrieWalker<TKey,TValue>
    {
        private readonly Trie<TKey, TValue> _trie;

        public TrieWalker(Trie<TKey, TValue> trie)
        {
            _trie = trie;
            CurrentNode = _trie;
        }

        private TrieNode<TKey, TValue> CurrentNode { get; set; }
        public IEnumerable<TValue> CurrentValues => CurrentNode.Values();

        public bool TryGoToChild(TKey key)
        {
            var node = CurrentNode.GetChildOrNull(key);
            if (node == null) return false;
            CurrentNode = node;
            return true;
        }

        public bool TryGoToChild(Func<TKey,TKey,TKey> aggregateFunc)
        {
            var node = CurrentNode.GetChildOrNull(aggregateFunc);
            if (node == null) return false;
            CurrentNode = node;
            return true;
        }

        public void GoToRoot()
        {
            CurrentNode = _trie;
        }
    }
}
