namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class TrieNode<TKey, TValue>
{
    public void Add(IList<TKey> query, int position, TValue value)
    {
        ArgumentNullException.ThrowIfNull(query);
        //ArgumentOutOfRangeException.th

        if (OutOfKeySequence(position, query))
        {
            _values.Add(value);
            return;
        }

        var child = GetOrCreateChild(query[position]);
        child.Add(query, position + 1, value);
    }

    internal void CleanPath(IList<TKey> query, int position)
    {
        ArgumentNullException.ThrowIfNull(query);

        TrieNode<TKey, TValue>? candidate = null;

        var key = default(TKey);

        do
        {
            var isRemovable = _values.Count == 0 && (
                   position < query.Count && _childrenDictionary.Count == 1 &&  _childrenDictionary.ContainsKey(query[position]) ||
                   position == query.Count && _childrenDictionary.Count == 0
               );

            if (isRemovable && candidate == null)
            {
                candidate = GetChildOrNull(query[position]);
                key = query[position];
            }
            else
                candidate = null;

        } while (!OutOfKeySequence(position++, query));

        if (candidate?.Parent != null)
        {
            candidate.Parent._childrenDictionary.Remove(key!);
        }

    }

    internal bool Remove(IList<TKey> query, int position, Predicate<TValue> predicate)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (OutOfKeySequence(position, query))
        {
            return RemoveValue(predicate);
        }

        var node = GetChildOrNull(query[position]);
        return node != null && node.Remove(query, position + 1, predicate);
    }

    private bool RemoveValue(Predicate<TValue> predicate)
    {
        if (predicate == null)
        {
            _values.Clear();
            return true;
        }

        var i = _values.FirstOrDefault(v => predicate(v));
        if (object.Equals(i, default(TValue))) return false;
        _values.Remove(i);
        return true;
    }

    private TrieNode<TKey, TValue> GetOrCreateChild(TKey childKey)
    {
        if (_childrenDictionary.TryGetValue(childKey, out var child))
        {
            child.Key.TriggerKey.Handled = childKey.TriggerKey.Handled;
            return child;
        }

        child = new TrieNode<TKey, TValue>(childKey, this);
        _childrenDictionary.Add(childKey, child);
        return child;
    }

    internal IEnumerable<TValue> Get(IList<TKey> query, int position)
    {
        return OutOfKeySequence(position, query)
            ? AllSubtreeValues()
            : SearchDeep(query, position);
    }

    private IEnumerable<TValue> SearchDeep(IList<TKey> query, int position)
    {
        var nextNode = GetChildOrNull(query[position]);

        return nextNode != null
            ? nextNode.Get(query, position + 1)
            : [];
    }

    private static bool OutOfKeySequence(int position, ICollection<TKey> query)
    {
        return position >= query.Count;
    }

    private IEnumerable<TValue> AllSubtreeValues()
    {
        return AllSubtreeNodes().SelectMany(node => node.Values);
    }

    private IEnumerable<TrieNode<TKey, TValue>> AllSubtreeNodes()
    {
        return Enumerable.Repeat(this, 1).Concat(ChildrenDictionary.Values.SelectMany(child => child.AllSubtreeNodes()));
    }
}