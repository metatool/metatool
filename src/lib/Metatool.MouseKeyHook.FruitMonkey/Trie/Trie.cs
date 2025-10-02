using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class Trie<TKey, TFruit> where TKey : ICombination where TFruit : KeyEventCommand
{
    private readonly TrieNode<TKey, TFruit> _root = new TrieNode<TKey, TFruit>(default(TKey));
    public Trie()
    {
        currentNode = _root;
    }

    private TrieNode<TKey, TFruit> currentNode;
    internal TrieNode<TKey, TFruit> CurrentNode
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