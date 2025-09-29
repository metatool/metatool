namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class TrieNode<TKey, TValue> 
{
	public void Add(IList<TKey> query, int position, TValue value)
	{
        ArgumentNullException.ThrowIfNull(query);

        if (OutOfKeySequence(position, query))
		{
			AddValue(value);
			return;
		}

		var child = GetOrCreateChild(query[position]);
		child.Add(query, position + 1, value);
	}

    private void AddValue(TValue value)
    {
        _values.Add(value);
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

	protected TrieNode<TKey, TValue> CleanPath(IList<TKey> query, int position)
	{
        ArgumentNullException.ThrowIfNull(query);

        TrieNode<TKey, TValue> candidate = null;
		TrieNode<TKey, TValue> parent = null;
		var key = default(TKey);

		do
		{
			if (IsRemovable(query, position) && candidate == null)
			{
				candidate = GetChildOrNull(query, position);
				parent = this;
				key = query[position];
			}
			else
				candidate = null;

		} while (!OutOfKeySequence(position++, query));

		if (candidate != null)
		{
			parent._childrenDictionary.Remove(key);
        }

		return candidate;
	}

	protected bool Remove(IList<TKey> query, int position, Predicate<TValue> predicate)
	{
        ArgumentNullException.ThrowIfNull(query);

        if (OutOfKeySequence(position, query))
		{
			return RemoveValue(predicate);
		}

		var node = GetChildOrNull(query, position);
		return node != null && node.Remove(query, position + 1, predicate);
	}

    protected TrieNode<TKey, TValue> GetOrCreateChild(TKey childKey)
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

    protected internal virtual IEnumerable<TValue> Get(IList<TKey> query, int position)
	{
		return OutOfKeySequence(position, query)
			? ValuesDeep()
			: SearchDeep(query, position);
	}

	protected virtual IEnumerable<TValue> SearchDeep(IList<TKey> query, int position)
	{
		var nextNode = GetChildOrNull(query, position);
		return nextNode != null
			? nextNode.Get(query, position + 1)
			: [];
	}

	private static bool OutOfKeySequence(int position, ICollection<TKey> query)
	{
		return position >= query.Count;
	}

	private IEnumerable<TValue> ValuesDeep()
	{
		return Subtree().SelectMany(node => node.Values);
	}

	protected IEnumerable<TrieNode<TKey, TValue>> Subtree()
	{
		return Enumerable.Repeat(this, 1).Concat(Children.SelectMany(child => child.Subtree()));
	}
}