namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public interface ITrie<TKey,TValue>
{
	void Add(IList<TKey> query, TValue value);
	IEnumerable<TValue> Get(IList<TKey> query);
	bool Remove(IList<TKey> query, Predicate<TValue>? predicate = null);
	void Clear();

}