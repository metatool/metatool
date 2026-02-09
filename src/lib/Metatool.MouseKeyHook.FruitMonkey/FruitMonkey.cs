using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Metatool.Input;

public class FruitMonkey(ILogger logger, IKeyTipNotifier notify) : IFruitMonkey
{
    // for sequence keys: A,B (B is a child of A)
    List<SelectionResult> _selectedResults = new();
    private readonly Forest _forest = new(notify, logger);
    public IForest Forest => _forest;

    public void Reset()
    {
        foreach (var stateTree in _forest.ForestGround.Values)
            stateTree.Reset();
    }

    List<SelectionResult> SelectTrees(IKeyEventArgs args)
    {
        var selectionResults = new List<SelectionResult>();
        //all on root, find current trees
        foreach (var stateTree in _forest.ForestGround.Values)
        {
            Debug.Assert(stateTree.IsOnRoot);

            if (stateTree.ClimbingState == TreeClimbingState.LandingAndClimbOthers)
                continue;

            TrieNode<ICombination, KeyEventCommand>? candidateNode = null;
            var state = stateTree.TrySelectChildNode(args, ref candidateNode);
            if (candidateNode == null)
                continue;

            selectionResults.Add(new SelectionResult(stateTree, candidateNode, state));
            if (selectionResults.Count == 0)
            {
            }
            else if (candidateNode.Key.ChordCount > selectionResults[0].SelectedNode!.Key.ChordCount)// both A+B+C and A+C matched, prefer A+B+C
            {
                selectionResults.Clear();
                selectionResults.Add(new SelectionResult(stateTree, candidateNode, state));
            }
            // note: may select several trees(hotkey with no chords). i.e. Caps to select ChoreMap and Map
        }

        if (selectionResults.Count > 0)
            logger.LogInformation($"TreeSelected:\n{string.Join(",\n", selectionResults.Select(t => $"{{tree:{t.Tree.Name},node:{{{t.SelectedNode}}}}}"))}");

        return selectionResults;
    }

    public void ClimbTree(IKeyEventArgs args)
    {
        // * if tree1 has A+B and tree2 has A,B, and we press A+B, A+B on tree1 would be processed, but A on tree2 would not be process as the next event is not A_up but B_down.
        // * if tree1 has A and tree2 has A, both should be processed.
        // * the ground is the root of every tree, runs like all are in the same tree, but provide state jump for every tree
        bool reprocess;
        do
        {
            reprocess = false;
            var onGround = false;

            if (_selectedResults.Count == 0)
            {
                onGround = true;
                _selectedResults = SelectTrees(args);
            }
            else
            {
                logger.LogInformation($"NoTreeSelection, trees:\n{string.Join('\n',_selectedResults.Select(t => $"{{{t.Tree.Name},nodePath:{t.SelectedNode}}}"))} ");
            }

            var hasSelectedNodes = _selectedResults.Count > 0;
            if (!hasSelectedNodes) goto @return;

            var selectionsToRemove = new List<SelectionResult>();

            for (int i = 0; i < _selectedResults.Count; i++)
            {
                var selectionResult = _selectedResults[i];
                if (!onGround)
                {
                    TrieNode<ICombination, KeyEventCommand>? candidateNode = null;
                    var state = selectionResult.Tree.TrySelectChildNode(args, ref candidateNode);
                    _selectedResults[i] = selectionResult = new SelectionResult(selectionResult.Tree, candidateNode, state);
                }

                var treeState = selectionResult.TreeState;
                if (selectionResult.SelectedNode != null)
                {
                    treeState = selectionResult.Tree.Climb(args, selectionResult.SelectedNode);
                }

                logger.LogInformation($"\tTree:{selectionResult.Tree.Name},State:{treeState},NodePath:{selectionResult.Tree.CurrentNode.KeyPath}");
                if (treeState == TreeClimbingState.Continue)
                {
                    // continue on this tree
                }
                else if (treeState == TreeClimbingState.Done)
                {
                    selectionsToRemove.Add(selectionResult);
                }
                else if (treeState == TreeClimbingState.NoFurtherProcess)
                {
                    selectionsToRemove.Add(selectionResult);
                    goto @return;
                }
                else if (treeState == TreeClimbingState.LandingAndClimbAll || treeState == TreeClimbingState.LandingAndClimbOthers)
                {
                    _selectedResults.Remove(selectionResult);
                    reprocess = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            selectionsToRemove.ForEach(s => _selectedResults.Remove(s));
        } while (_selectedResults.Count == 0 && /*no TreeClimbingState.Continue*/
                 reprocess /*Landing or LandingAndClimbing*/);

    @return:
        foreach (var stateTree in _forest.ForestGround.Values)
            stateTree.MarkDoneIfLanding();
    }

}