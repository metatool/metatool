using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public interface ITrie<TKey,TFruit> where TKey : ICombination where TFruit : KeyEventCommand
{
	void Add(IList<TKey> query, TFruit value);
	IEnumerable<TFruit> GetFruits(IList<TKey> query);
	bool TryGet(IList<TKey> path, out TrieNode<TKey, TFruit>? node);

    bool Remove(IList<TKey> query, Predicate<TFruit>? predicate = null);
	void Clear();

}