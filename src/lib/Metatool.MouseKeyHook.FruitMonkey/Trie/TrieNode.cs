using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

/// <param name="_parent"> used to generate KeyPath</param>
public partial class TrieNode<TKey, TFruit>(TKey key, TrieNode<TKey, TFruit>? _parent = null) where TKey : ICombination where TFruit : KeyEventCommand
{
    public TKey Key { get; } = key;

    internal TrieNode<TKey, TFruit>? parent  = _parent;

    private readonly IList<TFruit> _values = new KeyActionList<TFruit>();
    protected internal IEnumerable<TFruit> Values => _values;

    private readonly Dictionary<TKey, TrieNode<TKey, TFruit>> _children = [];
    internal Dictionary<TKey, TrieNode<TKey, TFruit>> Children => _children;

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
        var k = Key == null ? // || Parent == null too
            "Root" :
            KeyPath.PathString();
        return $"{{KeyPath:{k}; {ValuesDescriptions}}}";
    }

    internal void Clear()
    {
        _children.Clear();
        _values.Clear();
    }

    private string ValuesDescriptions => $"Cmds:[{string.Join(",", _values)}]";

    internal IEnumerable<(string key, IEnumerable<string> descriptions)> Tip =>
        _children.Select(
            p => (
                $"{p.Key.KeyName}",
                p.Value._values
                    .Where(ea => !string.IsNullOrEmpty(ea.Command.Description))
                    .Select(ea =>
                        (ea.KeyEventType == KeyEventType.Up ? "↑ " : "↓ ") + ea.Command.Description)
            )
        );

}