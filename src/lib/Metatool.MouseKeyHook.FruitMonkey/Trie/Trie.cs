using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public class Trie<TKey,TValue> : TrieNode<TKey,TValue>, ITrie<TKey,TValue> where TKey:ICombination where TValue: KeyEventCommand
{
	public Trie() : base(default(TKey))
	{
    }
    public IEnumerable<TValue> Get(IList<TKey> query)
	{
		return Get(query, 0);
	}

	public void Add(IList<TKey> query, TValue value)
	{
		Add(query, 0, value);
	}

	public bool Remove(IList<TKey> query,Predicate<TValue>? predicate = null)
	{
		var r = Remove(query, 0, predicate);
		CleanPath(query, 0);
		return r;
	}

	public new void Clear()
	{
		base.Clear();
	}
}