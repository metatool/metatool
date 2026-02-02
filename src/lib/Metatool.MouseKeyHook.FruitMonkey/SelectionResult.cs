using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

/// <summary>
/// when selected node is null tree state is not null, and vice versa
/// </summary>
internal readonly record struct SelectionResult(KeyStateTree Tree, TrieNode<ICombination, KeyEventCommand>? SelectedNode, TreeClimbingState? TreeState)
{
}