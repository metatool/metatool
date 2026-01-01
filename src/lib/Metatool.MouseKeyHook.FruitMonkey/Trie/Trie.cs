using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class Trie<TKey, TFruit> where TKey : ICombination where TFruit : KeyEventCommand
{
    private readonly TrieNode<TKey, TFruit> _root = new TrieNode<TKey, TFruit>(default(TKey));
    public Trie()
    {
        _currentNode = _root;
    }

    internal TrieNode<TKey, TFruit> Root => _root;

    private TrieNode<TKey, TFruit> _currentNode;
    internal TrieNode<TKey, TFruit> CurrentNode
    {
        get => _currentNode;
        set
        {
            if (_currentNode == value)
                return;

            _currentNode = value;
            Console.WriteLine($"\t@{_currentNode}");
        }
    }

    public void GoToRoot() => CurrentNode = _root;

    public bool IsOnRoot => CurrentNode == _root;

}