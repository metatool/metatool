using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;
namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class Trie<TKey, TFruit> : ITrie<TKey, TFruit> where TKey : ICombination where TFruit : KeyEventCommand
{
    /// <summary>
    /// get all the fruits stored in the node specified by path, and all its subtree nodes if any, 
    /// raise exception if path is not existing
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerable<TFruit> GetFruits(IList<TKey> path) => _root.Get(path, 0);
    public bool TryGet(IList<TKey> path, out TrieNode<TKey, TFruit>? node) => _root.TryGet(path, 0, out node);
    /// <summary>
    /// start from root, and go to and set current state to the state specified by path
    /// </summary>
    public bool TryGoTo(IList<TKey> path, out TrieNode<TKey, TFruit>? node)
    {
        var r = _root.TryGet(path, 0, out node);

        if(r)
            CurrentNode = node!;
        return r;
    }

    /// <summary>
    /// add the value to the node specified by path, create the path if not existing
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    public void Add(IList<TKey> path, TFruit value) => _root.Add(path, 0, value);

    /// <summary>
    /// remove the fruit(s) stored in the node specified by path, 
    /// if predicate is provided, remove only the fruits matching.
    /// after removal, clean up the path to remove the node if no fruit on its node and its subtree.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool Remove(IList<TKey> query, Predicate<TFruit>? predicate = null)
    {
        var r = _root.Remove(query, 0, predicate);
        _root.CleanPath(query, 0);
        return r;
    }

    public void Clear() => _root.Clear();
}
