using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

internal record struct SelectionResult(KeyStateTree tree, TrieNode<ICombination, KeyEventCommand>? selectedNode, bool downInChord)
{
    internal KeyStateTree Tree = tree;
    internal TrieNode<ICombination, KeyEventCommand>? SelectedNode = selectedNode;
    internal bool DownInChord = downInChord;
}