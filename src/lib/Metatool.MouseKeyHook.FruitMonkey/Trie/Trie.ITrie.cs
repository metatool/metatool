using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;
namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class Trie<TKey, TValue> : ITrie<TKey, TValue> where TKey : ICombination where TValue : KeyEventCommand
{
    public IEnumerable<TValue> Get(IList<TKey> query) => _root.Get(query, 0);

    public void Add(IList<TKey> query, TValue value) => _root.Add(query, 0, value);

    public bool Remove(IList<TKey> query, Predicate<TValue>? predicate = null)
    {
        var r = _root.Remove(query, 0, predicate);
        _root.CleanPath(query, 0);
        return r;
    }

    public void Clear() => _root.Clear();
}
