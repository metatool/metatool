using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

internal struct SelectionResult(KeyStateTree tree, TrieNode<ICombination, KeyEventCommand> candidateNode, bool downInChord)
{
    internal KeyStateTree Tree = tree;
    internal TrieNode<ICombination, KeyEventCommand> CandidateNode = candidateNode;
    internal bool DownInChord = downInChord;
}