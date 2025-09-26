namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public abstract class TrieNodeBase<TKey, TValue>
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

	protected internal abstract IEnumerable<TValue> Values();

	protected abstract IEnumerable<TrieNodeBase<TKey, TValue>> Children { get; }

	protected abstract void RemoveChild(TKey key);
	protected internal abstract int ChildrenCount { get; }

	protected abstract void AddValue(TValue value);
	protected abstract bool RemoveValue(Predicate<TValue> predicate);

	protected abstract bool IsRemovable(IList<TKey> query, int position);

	protected TrieNodeBase<TKey, TValue> CleanPath(IList<TKey> query, int position)
	{
		if (query == null) throw new ArgumentNullException(nameof(query));

		TrieNodeBase<TKey, TValue> candidate = null;
		TrieNodeBase<TKey, TValue> parent = null;
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
			parent.RemoveChild(key);
		}

		return candidate;
	}


	protected bool Remove(IList<TKey> query, int position, Predicate<TValue> predicate)
	{
		if (query == null) throw new ArgumentNullException(nameof(query));

		if (OutOfKeySequence(position, query))
		{
			return RemoveValue(predicate);
		}

		var node = GetChildOrNull(query, position);
		return node != null && node.Remove(query, position + 1, predicate);
	}

	protected abstract TrieNodeBase<TKey, TValue> GetOrCreateChild(TKey childKey);

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

	protected abstract TrieNodeBase<TKey, TValue> GetChildOrNull(IList<TKey> query, int position);

	private static bool OutOfKeySequence(int position, ICollection<TKey> query)
	{
		return position >= query.Count;
	}

	private IEnumerable<TValue> ValuesDeep()
	{
		return Subtree().SelectMany(node => node.Values());
	}

	protected IEnumerable<TrieNodeBase<TKey, TValue>> Subtree()
	{
		return Enumerable.Repeat(this, 1).Concat(Children.SelectMany(child => child.Subtree()));
	}
}