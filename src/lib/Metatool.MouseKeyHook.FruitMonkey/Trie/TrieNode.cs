using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;


/// <param name="key"></param>
/// <param name="parent"> used to generate KeyPath</param>
public partial class TrieNode<TKey, TFruit>(TKey key, TrieNode<TKey, TFruit>? _parent = null) where TKey : ICombination where TFruit : KeyEventCommand
{
    public TKey Key { get; } = key;

    internal TrieNode<TKey, TFruit>? parent  = _parent;

    private readonly IList<TFruit> _values = new KeyActionList<TFruit>();
    protected internal IEnumerable<TFruit> Values => _values;

    private readonly Dictionary<TKey, TrieNode<TKey, TFruit>> _childrenDictionary = [];
    internal Dictionary<TKey, TrieNode<TKey, TFruit>> ChildrenDictionary => _childrenDictionary;

    private IKeyPath? _keyPath;
    public IKeyPath KeyPath
    {
        get
        {
            if (_keyPath != null) return _keyPath;

            if (Key == null) // root, Parent == null too
                return null;

            _keyPath = new Sequence(
                parent!.Key == null
                ? [Key]
                : [.. parent.KeyPath, Key]
            );

            return _keyPath;
        }
    }

    public override string ToString()
    {
        return Key == null ? // || Parent == null too
            "Root" :
            parent!.Key == null ?
                $"{Key}" :
                $"{parent.Key}, {Key}";
    }

    internal void Clear()
    {
        _childrenDictionary.Clear();
        _values.Clear();
    }

    internal IEnumerable<(string key, IEnumerable<string> descriptions)> Tip =>
        _childrenDictionary.Select(
            p => (
                $"{p.Key}",
                p.Value._values
                    .Where(ea => !string.IsNullOrEmpty(ea.Command.Description))
                    .Select(ea =>
                        (ea.KeyEventType == KeyEventType.Up ? "↑ " : "↓ ") + ea.Command.Description)
            )
        );

}