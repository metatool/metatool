using Metatool.MouseKeyHook.FruitMonkey;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Metatool.Input;

public class FruitMonkey(ILogger logger, IKeyTipNotifier notify): IFruitMonkey
{
    List<SelectionResult> _selectedTrees = new();
    private readonly Forest _forest = new (notify, logger);
    public IForest Forest => _forest;

    public void Reset()
    {
        foreach (var stateTree in _forest.ForestGround.Values) 
            stateTree.Reset();
    }

    List<SelectionResult> SelectTree(KeyEventType eventType, IKeyEventArgs args)
    {
        var selectionResults = new List<SelectionResult>();
        //all on root, find current trees
        foreach (var stateTree in _forest.ForestGround.Values)
        {
            Debug.Assert(stateTree.IsOnRoot);

            if (stateTree.ClimbingState == TreeClimbingState.Landing)
                continue;

            var selectionResult = stateTree.TrySelectNode(eventType, args);
            if (selectionResult.SelectedNode == null)
                continue;

            if (selectionResults.Count == 0)
            {
                selectionResults.Add(selectionResult);
            }
            else if (selectionResult.SelectedNode.Key.ChordCount > selectionResults[0].SelectedNode!.Key.ChordCount)
            {
                selectionResults.Clear();
                selectionResults.Add(selectionResult);
            }
        }

        if (selectionResults.Count > 0)
            logger.LogInformation(
                $"ToClimbTrees:{string.Join(",", selectionResults.Select(t => $"${t.Tree.Name}@{t.SelectedNode}"))}");

        return selectionResults;
    }

    public void ClimbTree(KeyEventType eventType, IKeyEventArgs args)
    {
        // * if tree1 has A+B and tree2 has A and B, and we press A+B, A+B on tree1 would be processed, but A on tree2 would not be process as the next event is not A_up but B_down.
        // * if tree1 has A and tree2 has A, both should be processed.
        // * the ground is the root of every tree, runs like all are in the same tree, but provide state jump for every tree
        bool reprocess;
        do
        {
            reprocess = false;
            var onGround = false;

            if (_selectedTrees.Count == 0)
            {
                onGround = true;
                _selectedTrees = SelectTree(eventType, args);
            }

            var hasSelectedNodes = _selectedTrees.Count > 0;
            if (!hasSelectedNodes) goto @return;

            List<SelectionResult> trees = [.. _selectedTrees]; // reference copy
            foreach (var c in trees)
            {
                var selectedTree = c; // should not remove this line
                if (!onGround)
                {
                    var result = selectedTree.Tree.TrySelectNode(eventType, args);
                    var index = _selectedTrees.IndexOf(selectedTree);
                    _selectedTrees[index] = result;
                    selectedTree = result;
                }

                var rt = selectedTree.Tree.Climb(eventType, args, selectedTree.SelectedNode, selectedTree.DownInChord);
                logger.LogInformation($"\t={rt}${selectedTree.Tree.Name}@{selectedTree.Tree.CurrentNode}");
                if (rt == TreeClimbingState.Continue)
                {
                    // continue on this tree
                }
                else if (rt == TreeClimbingState.Done)
                {
                    _selectedTrees.Remove(selectedTree);
                }
                else if (rt == TreeClimbingState.NoFurtherProcess)
                {
                    _selectedTrees.Remove(selectedTree);
                    goto @return;
                }
                else if (rt == TreeClimbingState.LandingAndClimbing || rt == TreeClimbingState.Landing)
                {
                    _selectedTrees.Remove(selectedTree);
                    reprocess = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        } while (_selectedTrees.Count == 0 && /*no TreeClimbingState.Continue*/
                 reprocess /*Landing or LandingAndClimbing*/);

    @return:
        foreach (var stateTree in _forest.ForestGround.Values) 
            stateTree.MarkDoneIfLanding();
    }

}