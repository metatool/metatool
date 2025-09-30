using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

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

        set
        {
            if (currentNode == value)
                return;

            currentNode = value;
            Console.WriteLine($"\t@{currentNode}");
        }
    }

    public void GoToRoot() => CurrentNode = _root;

    public bool IsOnRoot => CurrentNode == _root;

    /// <summary>
    /// start from root, and go to(set current state to) the state specified by path
    /// </summary>
    public bool TryGoToState(IKeyPath path, out TrieNode<TKey, TValue> node)
    {
        ArgumentNullException.ThrowIfNull(path);

        node = _root;

        foreach (var combination in path)
        {
            if (node.ChildrenDictionary.TryGetValue((TKey)combination, out var child))
            {
                node = child;
                continue;
            }

            return false;
        }

        CurrentNode = node;
        return true;
    }
}