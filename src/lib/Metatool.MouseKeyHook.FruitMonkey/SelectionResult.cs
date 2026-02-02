using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

internal readonly record struct SelectionResult(KeyStateTree Tree, TrieNode<ICombination, KeyEventCommand>? SelectedNode, TreeClimbingState treeState)
{
}