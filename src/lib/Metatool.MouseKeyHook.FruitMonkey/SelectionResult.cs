using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

internal struct SelectionResult
{
    public SelectionResult(KeyStateTree tree, TrieNode<ICombination, KeyEventCommand> candidateNode,
        bool downInChord)
    {
        Tree = tree;
        CandidateNode = candidateNode;
        DownInChord = downInChord;
    }

    internal KeyStateTree Tree;
    internal TrieNode<ICombination, KeyEventCommand> CandidateNode;
    internal bool DownInChord;
}