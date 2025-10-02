using Metatool.Service.MouseKey;
using System.IO;

namespace Metatool.MouseKeyHook.FruitMonkey.Trie;

/// <summary>
/// the branch node(crotch) of the trie, it contains branches of child nodes
/// </summary>
public partial class TrieNode<TKey, TFruit>
{
    /// <summary>
    /// make sure all child branches exist and add the value to the end of the branch indicated by query
    /// </summary>
    /// <param name="path">the branch path</param>
    /// <param name="position">the start position of the current node in the path</param>
    /// <param name="fruit"></param>
    public void Add(IList<TKey> path, int position, TFruit fruit)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, path.Count);

        if (position == path.Count)
        {
            _values.Add(fruit);
            return;
        }

        var childKey = path[position];
        var child = GetOrCreateChild(childKey);

        child.Key.TriggerKey.Handled = childKey.TriggerKey.Handled;
        child.Add(path, position + 1, fruit);
    }
    private TrieNode<TKey, TFruit> GetOrCreateChild(TKey childKey)
    {
        if (_childrenDictionary.TryGetValue(childKey, out var child))
        {
            return child;
        }

        child = new TrieNode<TKey, TFruit>(childKey, this);
        _childrenDictionary.Add(childKey, child);
        return child;
    }

    /// <summary>
    /// get the fruits stored in the node specified by path, and all its subtree nodes if any
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position">the position of the current node in the path</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    internal IEnumerable<TFruit> Get(IList<TKey> path, int position)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, path.Count);

        if (position == path.Count)
        {
            return FlatCurrentAndSubtreeNodes().SelectMany(node => node.Values);
        }
        else
        {
            var key = path[position];
            _childrenDictionary.TryGetValue(key, out var child);

            if (child == null)
                throw new KeyNotFoundException($"Get: Key '{key}' in the path: {path} is not found in Trie");

            return child.Get(path, position + 1);
        }
    }

    /// <summary>
    /// try to go to the node specified by path, if found, set node to it and return true, otherwise set node to null and return false
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position">the index of the current node in path</param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool TryGoTo(IList<TKey> path, int position, out TrieNode<TKey, TFruit>? node)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, path.Count);

        node = this;
        for (var i = position; i < path.Count; i++)
        {
            var key = path[i];
            if (node.ChildrenDictionary.TryGetValue(key, out var child))
            {
                node = child;
                continue;
            }

            node = null;
            return false;
        }

        return true;
    }

    private IEnumerable<TrieNode<TKey, TFruit>> FlatCurrentAndSubtreeNodes()
    {
        return Enumerable.Repeat(this, 1).Concat(ChildrenDictionary.Values.SelectMany(child => child.FlatCurrentAndSubtreeNodes()));
    }

    /// <summary>
    /// remove the node on the path from the trie if no fruit is stored in it and its subtree
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    /// <returns>true: if the whole path has no fruit</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    internal bool CleanPath(IList<TKey> path, int position)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, path.Count);

        if (position == path.Count)
        {
            return RemoveFromParentIfNoFruit();
        }
        else
        {
            var key = path[position];
            _childrenDictionary.TryGetValue(key, out var child);

            if (child == null)
                throw new KeyNotFoundException($"CleanPath: Key '{key}' in the path: {path} is not found in Trie");

            if(child.CleanPath(path, position + 1))
            {
                return RemoveFromParentIfNoFruit();
            } 
            else
            {
                return false;
            }
        }
    }

    private bool RemoveFromParentIfNoFruit()
    {
        if (_values.Count == 0)
        {
            parent!._childrenDictionary.Remove(Key);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// remove the fruit(s) stored in the node specified by path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position">the position of the current node in the path</param>
    /// <param name="predicate"> to select the value to remove, null: all fruits</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    internal bool Remove(IList<TKey> path, int position, Predicate<TFruit>? predicate)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, path.Count);

        if (position == path.Count)
        {
            return RemoveFistValue(predicate);
        }
        _childrenDictionary.TryGetValue(path[position], out var child);

        if (child == null)
            throw new KeyNotFoundException($"Get: Key '{key}' in the path: {path} is not found in Trie");

        return child.Remove(path, position + 1, predicate);
    }

    private bool RemoveFistValue(Predicate<TFruit>? predicate)
    {
        if (predicate == null)
        {
            _values.Clear();
            return true;
        }

        var i = _values.FirstOrDefault(v => predicate(v));
        if (object.Equals(i, default(TFruit)))
            return false;

        _values.Remove(i);
        return true;
    }

}