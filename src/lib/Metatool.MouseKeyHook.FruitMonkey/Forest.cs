using Metatool.Input;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey;

public class Forest(IKeyTipNotifier notify): IForest
{
    internal Dictionary<string, KeyStateTree> ForestGround = new()
    {
		// keep the order
		{KeyStateTrees.HardMap, new KeyStateTree(KeyStateTrees.HardMap, notify)},
        {KeyStateTrees.ChordMap, new KeyStateTree(KeyStateTrees.ChordMap, notify) {TreeType = TreeType.SingleFruitPerEventType}},
        {KeyStateTrees.Default, new KeyStateTree(KeyStateTrees.Default, notify)},
        {KeyStateTrees.Map, new KeyStateTree(KeyStateTrees.Map, notify)},
        {KeyStateTrees.HotString, new KeyStateTree(KeyStateTrees.HotString, notify)}
    };

    internal KeyStateTree GetOrCreateStateTree(string stateTreeName)
    {
        if (ForestGround.TryGetValue(stateTreeName, out var keyStateTree))
        {
            return keyStateTree;
        }

        keyStateTree = new KeyStateTree(stateTreeName, notify);
        ForestGround.Add(stateTreeName, keyStateTree);
        return keyStateTree;
    }

    public void DisableChord(Chord chord, string? stateTreeName = null)
    {
        if (stateTreeName == null)
        {
            foreach (var tree in ForestGround.Values)
            {
                tree.DisableChord(chord);
            }
            return;
        }
        var stateTree = GetOrCreateStateTree(stateTreeName);
        stateTree.DisableChord(chord);
    }

    public void EnableChord(Chord chord, string? stateTree = null)
    {
        if (stateTree == null)
        {
            foreach (var tree in ForestGround.Values)
            {
                tree.EnableChord(chord);
            }
            return;
        }
        var tre = GetOrCreateStateTree(stateTree);
        tre.EnableChord(chord);
    }

    public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command, string stateTree = KeyStateTrees.Default)
    {
        var keyStateTree = GetOrCreateStateTree(stateTree);
        return keyStateTree.Add(combinations, command);
    }

    public bool Contains(IHotkey hotKey, string? stateTree = null)
    {
        if (stateTree == null)
        {
            foreach (var tree in ForestGround.Values)
            {
                if (tree.Contains(hotKey))
                    return true;
            }
            return false;
        }
        var tre = GetOrCreateStateTree(stateTree);
        return tre.Contains(hotKey);
    }

    public void ShowTip(bool ifRootThenEmpty = false)
    {
        var tips =ForestGround.Values.SelectMany(m => m.Tips(ifRootThenEmpty)).ToArray();
        if (tips.Length > 0)
            notify.ShowKeysTip("Forest",tips);
        else
        {
            notify.CloseKeysTip("Forest");
        }
    }
}
