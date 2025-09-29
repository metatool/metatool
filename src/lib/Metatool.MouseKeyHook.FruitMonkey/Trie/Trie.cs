using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class Trie<TKey, TValue>: ITrie<TKey, TValue> where TKey : ICombination where TValue : KeyEventCommand
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

public partial class Trie<TKey, TValue> where TKey : ICombination where TValue : KeyEventCommand
{
    private readonly TrieNode<TKey, TValue> _root = new TrieNode<TKey, TValue>(default(TKey));
    public Trie()
    {
        currentNode = _root;
    }

    private TrieNode<TKey, TValue> currentNode;
    internal TrieNode<TKey, TValue> CurrentNode
    {
        get => currentNode;

        private set
        {
            if (currentNode == value)
                return;

            currentNode = value;
            Console.WriteLine($"\t@{currentNode}");
        }
    }

    public bool IsOnRoot => CurrentNode == _root;

    public int CurrentChildrenCount => CurrentNode.ChildrenCount;

    public IEnumerable<TValue> CurrentValues => CurrentNode.Values;

    public bool TryGoToChild(TKey key)
    {
        var node = CurrentNode.GetChildOrNull(key);
        if (node == null) 
            return false;

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

    internal TrieNode<TKey, TValue> GetChildOrNull(Func<TKey, TKey, TKey> aggregateFunc, TKey initialKey = default(TKey))
    {
        return CurrentNode.GetChildOrNull(initialKey, aggregateFunc);
    }

    public void GoToChild(TrieNode<TKey, TValue> child) => CurrentNode = child;

    public void GoToRoot() => CurrentNode = _root;

    /// <summary>
    /// start from root, and go to(set current state to) the state specified by path
    /// </summary>
    public bool TryGoToState(IKeyPath path, out TrieNode<TKey, TValue> node)
    {
        node = _root;

        if (path != null)
        {
            foreach (var combination in path)
            {
                if (node.ChildrenDictionary.TryGetValue((TKey)combination, out var child))
                {
                    node = child;
                    continue;
                }

                return false;
            }
        }

        CurrentNode = node;
        return true;
    }
}