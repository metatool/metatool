using System;
using System.Collections.Generic;
using System.Linq;
using Metatool.DataStructures;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;

namespace Metatool.MouseKeyHook.Implementation.Trie;

public class TrieNode<TKey, TValue> : TrieNodeBase<TKey, TValue>
	where TKey : ICombination where TValue : KeyEventCommand
{
	private readonly Dictionary<TKey, TrieNode<TKey, TValue>> _childrenDictionary;
	private readonly IList<TValue> _values = new KeyActionList<TValue>();
	public TKey Key;
	public TrieNode<TKey, TValue> Parent;
	private IKeyPath _keyPath;

	public IKeyPath KeyPath
	{
		get
		{
			if (_keyPath != null) return _keyPath;
			if (Key == null || Parent == null)
				return null;
			_keyPath = new Sequence(Parent.Key == null
				? new ICombination[] {Key}
				: Parent.KeyPath.Concat(Key).ToArray());
			return _keyPath;
		}
	}

	public override string ToString()
	{
		return Key == null || Parent == null ? "Root" :
			Parent.Key == null ? $"{Key}" :
			$"{Parent.Key}, {Key}";
	}

	protected TrieNode(TKey key = default(TKey))
	{
		Key = key;
		_childrenDictionary = new Dictionary<TKey, TrieNode<TKey, TValue>>();
	}

	internal Dictionary<TKey, TrieNode<TKey, TValue>> ChildrenDictionaryPairs => _childrenDictionary;


	internal virtual void Clear()
	{
		_childrenDictionary.Clear();
		_values.Clear();
	}

	protected internal override int ChildrenCount => _childrenDictionary.Count;


	internal IEnumerable<(string key, IEnumerable<string> descriptions)> Tip => _childrenDictionary.Select(p =>
		(p.Key.ToString(),
			p.Value._values.Where(ea => !string.IsNullOrEmpty(ea.Command.Description)).Select(ea =>
				(ea.KeyEvent == KeyEvent.Up ? "↑ " : "↓ ") + ea.Command.Description)));

	protected override IEnumerable<TrieNodeBase<TKey, TValue>> Children => _childrenDictionary.Values;

	protected internal override IEnumerable<TValue> Values()
	{
		return _values;
	}

	protected override bool IsRemovable(IList<TKey> query, int position)
	{
		return position < query.Count && _childrenDictionary.Count == 1 && _values.Count == 0 &&
		       _childrenDictionary.ContainsKey(query[position]) ||
		       position == query.Count && _values.Count == 0 && _childrenDictionary.Count == 0;
	}

	protected override TrieNodeBase<TKey, TValue> GetOrCreateChild(TKey key)
	{
		if (_childrenDictionary.TryGetValue(key, out var child))
		{
			child.Key.TriggerKey.Handled = key.TriggerKey.Handled;
			return child;
		}

		child = new TrieNode<TKey, TValue>(key);
		child.Parent = this;
		_childrenDictionary.Add(key, child);
		return child;
	}

	internal TrieNode<TKey, TValue> GetChildOrNull(TKey initKey, Func<TKey, TKey, TKey> aggregateFunc)
	{
		var key = _childrenDictionary.Keys.Aggregate(initKey, aggregateFunc);

		if (EqualityComparer<TKey>.Default.Equals(key, default(TKey))) return null;
		return GetChildOrNull(key);
	}

	internal TrieNode<TKey, TValue> GetChildOrNull(TKey key)
	{
		return TryGetChild(key, out var childNode)
			? childNode
			: null;
	}

	internal bool TryGetChild(TKey key, out TrieNode<TKey, TValue> child)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		return _childrenDictionary.TryGetValue(key, out child);
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

		var i = _values.FirstOrDefault(v => predicate(v));
		if (object.Equals(i, default(TValue))) return false;
		_values.Remove(i);
		return true;
	}

	protected override void RemoveChild(TKey key)
	{
		_childrenDictionary.Remove(key);
	}
}