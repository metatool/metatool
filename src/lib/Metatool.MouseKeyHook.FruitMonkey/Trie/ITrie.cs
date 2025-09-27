namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

/// <summary>
/// Interface to be implemented by a data structure 
/// which allows adding values <see cref="TValue"/> associated with keys.
/// The interface allows retrieval of multiple values 
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface ITrie<TKey,TValue>
{
	void Add(IList<TKey> query, TValue value);
	IEnumerable<TValue> Get(IList<TKey> query);
	bool Remove(IList<TKey> query, Predicate<TValue>? predicate = null);
	void Clear();

}