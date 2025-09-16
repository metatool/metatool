using System;
using System.Collections.Generic;
using System.Linq;
using Metatool.DataStructures;
using Metatool.MouseKeyHook.Implementation.Trie;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation.Trie;

public class TrieWalker<TKey, TValue> where TKey : ICombination where TValue : KeyEventCommand
{
	private readonly Trie<TKey, TValue> _trie;

	public TrieWalker(Trie<TKey, TValue> trie)
	{
		_trie       = trie;
		_CurrentNode = _trie;
	}

	private TrieNode<TKey, TValue> _CurrentNode;

	internal TrieNode<TKey, TValue> CurrentNode
	{
		get => _CurrentNode;
		set
		{
			if (_CurrentNode == value) return;
			_CurrentNode = value;
			Console.WriteLine($"\t@{_CurrentNode}");
		}
	}

	public TrieNode<TKey, TValue> Root     => _trie;
	public bool                   IsOnRoot => CurrentNode == _trie;

	public int                 CurrentChildrenCount => CurrentNode.ChildrenCount;
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
		var node = CurrentNode.GetChildOrNull(initialKey, aggregateFunc);
		if (node == null) return false;
		CurrentNode = node;
		return true;
	}

	internal TrieNode<TKey, TValue> GetChildOrNull(Func<TKey, TKey, TKey> aggregateFunc,
		TKey initialKey = default(TKey))
	{
		return CurrentNode.GetChildOrNull(initialKey, aggregateFunc);
	}

	internal KeyValuePair<TKey, TrieNode<TKey, TValue>> GetChildOrNull(
		Func<KeyValuePair<TKey, TrieNode<TKey, TValue>>, KeyValuePair<TKey, TrieNode<TKey, TValue>>,
			KeyValuePair<TKey, TrieNode<TKey, TValue>>> aggregateFunc,
		KeyValuePair<TKey, TrieNode<TKey, TValue>> initKey = default(KeyValuePair<TKey, TrieNode<TKey, TValue>>))
	{
		return CurrentNode.ChildrenDictionaryPairs.Aggregate(initKey, aggregateFunc);
	}

	public void GoToChild(TrieNode<TKey, TValue> child)
	{
		CurrentNode = child;
	}

	public void GoToRoot()
	{
		CurrentNode = _trie;
	}

	public bool TryGoToState(IKeyPath path, out TrieNode<TKey, TValue> node)
	{
		node = _trie;

		if (path != null)
			foreach (var combination in path)
			{
				if (node.TryGetChild((TKey) combination, out var child))
				{
					node = child;
					continue;
				}

				return false;
			}

		CurrentNode = node;
		return true;
	}
}