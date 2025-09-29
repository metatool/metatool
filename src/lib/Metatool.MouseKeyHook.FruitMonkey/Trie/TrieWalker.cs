using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public class TrieWalker<TKey, TValue>(Trie<TKey, TValue> trie) where TKey : ICombination where TValue : KeyEventCommand
{
    private readonly Trie<TKey, TValue> _root = trie;

    private TrieNode<TKey, TValue> currentNode = trie;
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

    public IEnumerable<TValue> CurrentValues => CurrentNode.Values();

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
                if (node.TryGetChild((TKey)combination, out var child))
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