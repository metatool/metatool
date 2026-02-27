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

            var candidateNode = stateTree.TryFindChildNode(args);
            if (candidateNode == null)
                continue;

            if (selectionResults.Count == 0)
            {
                selectionResults.Add(new SelectionResult(stateTree, candidateNode));
            }
            else if (candidateNode.Key.ChordCount > selectionResults[0].CandidateNode!.Key.ChordCount)// both A+B+C and A+C matched, prefer A+B+C
            {
                selectionResults.Clear();
                selectionResults.Add(new SelectionResult(stateTree, candidateNode));
            }
            // note: may select several trees(hotkey with no chords). i.e. Caps to select ChoreMap and Map
        }

        if (selectionResults.Count > 0)
            logger.LogInformation($"\tTreeSelected:\n\t{string.Join(",\n\t\t", selectionResults.Select(t => $"candidateNode:{t.CandidateNode}, tree:{t.Tree}"))}");

        return selectionResults;
    }

    public void ClimbTree(IKeyEventArgs args)
    {
        //if (args.KeyCode == KeyCodes.Home && args.KeyEventType == KeyEventType.Down ) Debugger.Break();
        foreach (var stateTree in _forest.ForestGround.Values)
            stateTree.MarkDoneIfLanding();

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
                logger.LogInformation($"\tNoTreeSelection, AlreadySelectedTrees:\n\t{string.Join("\n\t\t", _selectedResults.Select(t => $"candidateNode:{t.CandidateNode}, tree:{t.Tree}"))} ");
            }

            var hasSelectedNodes = _selectedResults.Count > 0;
            if (!hasSelectedNodes) return;

            var selectionsToRemove = new List<SelectionResult>();

            for (int i = 0; i < _selectedResults.Count; i++)
            {
                var selectionResult = _selectedResults[i];
                if (!onGround)
                {
                    var candidateNode = selectionResult.Tree.TryFindChildNode(args);
                    _selectedResults[i] = selectionResult = new SelectionResult(selectionResult.Tree, candidateNode);
                }

                if (selectionResult.CandidateNode != null)
                {
                    selectionResult.Tree.Climb(args, selectionResult.CandidateNode);
                }

                var treeState = selectionResult.Tree.ClimbingState;
                logger.LogInformation($"\tAfterExecutionAndClimbingTree:{selectionResult.Tree}");
                switch (treeState)
                {
                    case TreeClimbingState.Continue_ChordDown_WaitForTrigger:
                    case TreeClimbingState.Continue_TriggerDown_WaitForUp:
                    case TreeClimbingState.Continue_ChordUp_WaitForTriggerOrOtherChordUp:
                    case TreeClimbingState.Continue_ChordUp_TriggerAlreadyUp_WaitForChildKeys:
                    case TreeClimbingState.Continue_TriggerUp_WaitForChordUpForAllUp:
                    case TreeClimbingState.Continue_TriggerUp_WaitForChildKeys:
                    case TreeClimbingState.Continue_AllUp_WaitForChildKeys:
                    case TreeClimbingState.Continue_AfterGoToPath:
                        // continue on this tree
                        break;
                    case TreeClimbingState.Done:
                        selectionsToRemove.Add(selectionResult);
                        break;
                    case TreeClimbingState.NoFurtherProcess:
                        selectionsToRemove.Add(selectionResult);
                        return;
                    case TreeClimbingState.LandingAndClimbAll:
                    case TreeClimbingState.LandingAndClimbOthers:
                        selectionsToRemove.Add(selectionResult);
                        reprocess = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            selectionsToRemove.ForEach(s => _selectedResults.Remove(s));
        } while (_selectedResults.Count == 0 && /*no TreeClimbingState.Continue*/
                 reprocess /*Landing or LandingAndClimbing*/);

    }

}