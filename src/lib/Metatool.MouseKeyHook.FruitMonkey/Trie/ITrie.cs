namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

/// <summary>
/// Interface to be implemented by a data structure 
/// which allows adding values <see cref="TValue"/> associated with keys.
/// The interface allows retrieval of multiple values 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface ITrie<TKey,TValue>
{
	IEnumerable<TValue> Get(IList<TKey> query);
	void Add(IList<TKey> query, TValue value);
	bool Remove(IList<TKey> query, Predicate<TValue>? predicate = null);
	void Clear();

}