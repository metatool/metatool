using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service.MouseKey;

namespace Metatool.MouseKeyHook.FruitMonkey;

public interface IForest
{
    IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command, string stateTree = KeyStateTrees.Default);

    bool Contains(IHotkey hotKey, string? stateTree = null);
    void DisableChord(Chord chord, string? stateTree = null);
    void EnableChord(Chord chord, string? stateTree = null);
    void ShowTip(bool ifRootThenEmpty = false);
}
