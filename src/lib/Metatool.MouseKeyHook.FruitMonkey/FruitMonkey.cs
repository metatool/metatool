using Metatool.MouseKeyHook.FruitMonkey;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Metatool.Input;

public class FruitMonkey(ILogger logger, IKeyTipNotifier notify): IFruitMonkey
{
    List<SelectionResult> selectedTrees = new();
    Forest forest = new (notify);
    public IForest Forest => forest;

    public void Reset()
    {
        foreach (var stateTree in forest.ForestGround.Values) 
            stateTree.Reset();
    }

    List<SelectionResult> SelectTree(KeyEventType eventType, IKeyEventArgs args, ILogger logger)
    {
        var selectedNodes = new List<SelectionResult>();
        //all on root, find current trees
        foreach (var stateTree in forest.ForestGround.Values)
        {
            Debug.Assert(stateTree.IsOnRoot);
            if (stateTree.ProcessState == KeyProcessState.Yield) continue;

            var selectionResult = stateTree.TrySelect(eventType, args);
            if (selectionResult.CandidateNode == null) continue;

            if (selectedNodes.Count == 0)
            {
                selectedNodes.Add(selectionResult);
            }
            else if (selectionResult.CandidateNode.Key.ChordLength > selectedNodes[0].CandidateNode.Key.ChordLength)
            {
                selectedNodes.Clear();
                selectedNodes.Add(selectionResult);
            }
        }

        if (selectedNodes.Count > 0)
            logger.LogInformation(
                $"ToClimb:{string.Join(",", selectedNodes.Select(t => $"${t.Tree.Name}_{t.CandidateNode}"))}");

        return selectedNodes;
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
            if (selectedTrees.Count == 0)
            {
                onGround = true;
                selectedTrees = SelectTree(eventType, args, logger);
            }

            var hasSelectedNodes = selectedTrees.Count > 0;
            if (!hasSelectedNodes) goto @return;

            var trees = selectedTrees.GetRange(0, selectedTrees.Count);
            foreach (var c in trees)
            {
                var selectedTree = c; // should not remove this line
                if (!onGround)
                {
                    var result = selectedTree.Tree.TrySelect(eventType, args);
                    var index = selectedTrees.IndexOf(selectedTree);
                    selectedTrees[index] = result;
                    selectedTree = result;
                }

                var rt = selectedTree.Tree.Climb(eventType, args, selectedTree.CandidateNode,
                    selectedTree.DownInChord);
                logger.LogInformation($"\t={rt}${selectedTree.Tree.Name}@{selectedTree.Tree.CurrentNode}");
                if (rt == KeyProcessState.Continue)
                {
                }
                else if (rt == KeyProcessState.Done)
                {
                    selectedTrees.Remove(selectedTree);
                }
                else if (rt == KeyProcessState.NoFurtherProcess)
                {
                    selectedTrees.Remove(selectedTree);
                    goto @return;
                }
                else if (rt == KeyProcessState.Reprocess || rt == KeyProcessState.Yield)
                {
                    selectedTrees.Remove(selectedTree);
                    reprocess = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        } while (selectedTrees.Count == 0 && /*no KeyProcessState.Continue*/
                 reprocess /*Yield or Reprocess*/);

        @return:
        foreach (var stateTree in forest.ForestGround.Values) stateTree.MarkDoneIfYield();
    }

}