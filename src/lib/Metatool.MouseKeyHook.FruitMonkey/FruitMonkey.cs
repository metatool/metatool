using Metatool.MouseKeyHook.FruitMonkey;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Metatool.Input;

public class FruitMonkey(ILogger logger, IKeyTipNotifier notify): IFruitMonkey
{
    List<SelectionResult> _selectedResults = new();
    private readonly Forest _forest = new (notify, logger);
    public IForest Forest => _forest;

    public void Reset()
    {
        foreach (var stateTree in _forest.ForestGround.Values)
            stateTree.Reset();
    }

    List<SelectionResult> SelectTree(IKeyEventArgs args)
    {
        var selectionResults = new List<SelectionResult>();
        //all on root, find current trees
        foreach (var stateTree in _forest.ForestGround.Values)
        {
            Debug.Assert(stateTree.IsOnRoot);

            if (stateTree.ClimbingState == TreeClimbingState.Landing)
                continue;

            var selectionResult = stateTree.TrySelectChildNode(args);
            if (selectionResult.SelectedNode == null)
                continue;

            if (selectionResults.Count == 0)
            {
                selectionResults.Add(selectionResult);
            }
            else if (selectionResult.SelectedNode.Key.ChordCount > selectionResults[0].SelectedNode!.Key.ChordCount)// both A+B+C and A+C matched, prefer A+B+C
            {
                selectionResults.Clear();
                selectionResults.Add(selectionResult);
            }
        }

        if (selectionResults.Count > 0)
            logger.LogInformation(
                $"TreeSelected:{string.Join(",", selectionResults.Select(t => $"{{tree:{t.Tree.Name},node:{t.SelectedNode}}}"))}");

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
                _selectedResults = SelectTree(args);
            }

            var hasSelectedNodes = _selectedResults.Count > 0;
            if (!hasSelectedNodes) goto @return;

            List<SelectionResult> selectionResults = [.. _selectedResults]; // snapshot
            foreach (var res in selectionResults)
            {
                var selectionResult = res; // should not remove this line
                if (!onGround)
                {
                    var newResult = selectionResult.Tree.TrySelectChildNode(args);
                    var index = _selectedResults.IndexOf(selectionResult);
                    _selectedResults[index] = newResult;
                    selectionResult = newResult;
                }

                var climbingResult = selectionResult.treeState;
                if(selectionResult.SelectedNode != null)
                {
                    climbingResult = selectionResult.Tree.Climb(args, selectionResult.SelectedNode);
                }

                logger.LogInformation($"\tClimbing result: {climbingResult}, on tree: {selectionResult.Tree.Name}, on node:{selectionResult.Tree.CurrentNode}");
                if (climbingResult == TreeClimbingState.Continue)
                {
                    // continue on this tree
                }
                else if (climbingResult == TreeClimbingState.Done)
                {
                    _selectedResults.Remove(selectionResult);
                }
                else if (climbingResult == TreeClimbingState.NoFurtherProcess)
                {
                    _selectedResults.Remove(selectionResult);
                    goto @return;
                }
                else if (climbingResult == TreeClimbingState.LandingAndClimbing || climbingResult == TreeClimbingState.Landing)
                {
                    _selectedResults.Remove(selectionResult);
                    reprocess = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        } while (_selectedResults.Count == 0 && /*no TreeClimbingState.Continue*/
                 reprocess /*Landing or LandingAndClimbing*/);

    @return:
        foreach (var stateTree in _forest.ForestGround.Values)
            stateTree.MarkDoneIfLanding();
    }

}