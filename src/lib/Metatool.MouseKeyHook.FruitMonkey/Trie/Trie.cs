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

}