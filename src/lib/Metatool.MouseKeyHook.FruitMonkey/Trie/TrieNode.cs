using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class TrieNode<TKey, TValue> where TKey : ICombination where TValue : KeyEventCommand
{
    internal TrieNode(TKey key, TrieNode<TKey, TValue>? parent = null)
    {
        Key = key;
        Parent = parent;
    }

    public TKey Key { get; }
    public TrieNode<TKey, TValue>? Parent { get; private set; }

    private readonly IList<TValue> _values = [];
    protected internal IEnumerable<TValue> Values => _values;

    private readonly Dictionary<TKey, TrieNode<TKey, TValue>> _childrenDictionary = [];
    internal Dictionary<TKey, TrieNode<TKey, TValue>> ChildrenDictionary => _childrenDictionary;

    protected internal int ChildrenCount => _childrenDictionary.Count;

    private IKeyPath? _keyPath;
    public IKeyPath KeyPath
    {
        get
        {
            if (_keyPath != null) return _keyPath;

            if (Key == null || Parent == null)
                return null;

            _keyPath = new Sequence(
                Parent.Key == null
                ? [Key]
                : [.. Parent.KeyPath, Key]
            );

            return _keyPath;
        }
    }

    public override string ToString()
    {
        return Key == null || Parent == null ?
            "Root" :
            Parent.Key == null ?
                $"{Key}" :
                $"{Parent.Key}, {Key}";
    }

    internal virtual void Clear()
    {
        _childrenDictionary.Clear();
        _values.Clear();
    }

    internal IEnumerable<(string key, IEnumerable<string> descriptions)> Tip => _childrenDictionary.Select(p =>
        (p.Key.ToString(),
            p.Value._values.Where(ea => !string.IsNullOrEmpty(ea.Command.Description)).Select(ea =>
                (ea.KeyEventType == KeyEventType.Up ? "↑ " : "↓ ") + ea.Command.Description)));

    protected IEnumerable<TrieNode<TKey, TValue>> Children => _childrenDictionary.Values;


    protected bool IsRemovable(IList<TKey> query, int position)
    {
        return position < query.Count && _childrenDictionary.Count == 1 && _values.Count == 0 &&
               _childrenDictionary.ContainsKey(query[position]) ||
               position == query.Count && _values.Count == 0 && _childrenDictionary.Count == 0;
    }

    internal TrieNode<TKey, TValue> GetChildOrNull(TKey initKey, Func<TKey, TKey, TKey> aggregateFunc)
    {
        var key = _childrenDictionary.Keys.Aggregate(initKey, aggregateFunc);

        if (EqualityComparer<TKey>.Default.Equals(key, default(TKey))) 
            return null;

        return GetChildOrNull(key);
    }

    internal TrieNode<TKey, TValue> GetChildOrNull(TKey key)
    {
        return _childrenDictionary.TryGetValue(key, out var childNode)
            ? childNode
            : null;
    }

    private TrieNode<TKey, TValue> GetChildOrNull(IList<TKey> query, int position)
    {
        return GetChildOrNull(query[position]);
    }

}