using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;
namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

public partial class Trie<TKey, TValue> : ITrie<TKey, TValue> where TKey : ICombination where TValue : KeyEventCommand
{
    /// <summary>
    /// get all the fruits stored in the node specified by path, and all its subtree nodes if any, 
    /// raise exception if path is not existing
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerable<TValue> Get(IList<TKey> path) => _root.Get(path, 0);

    /// <summary>
    /// start from root, and go to(set current state to) the state specified by path
    /// </summary>
    public bool TryGoTo(IList<TKey> path, out TrieNode<TKey, TValue>? node)
    {
        var r = _root.TryGoTo(path, 0, out node);

        if(r)
            CurrentNode = node!;
        return r;
    }

    /// <summary>
    /// add the value to the node specified by path, create the path if not existing
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    public void Add(IList<TKey> path, TValue value) => _root.Add(path, 0, value);

    /// <summary>
    /// remove the fruit(s) stored in the node specified by path, 
    /// if predicate is provided, remove only the fruits matching.
    /// after removal, clean up the path to remove the node if no fruit on its node and its subtree.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool Remove(IList<TKey> query, Predicate<TValue>? predicate = null)
    {
        var r = _root.Remove(query, 0, predicate);
        _root.CleanPath(query, 0);
        return r;
    }

    public void Clear() => _root.Clear();
}
