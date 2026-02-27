using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.Logging;

namespace Metatool.Input;

[DebuggerDisplay("${ToString()}")]
public class KeyStateTree
{
    public TreeType TreeType = TreeType.Default;

    private readonly Trie<ICombination, KeyEventCommand> _trie = new();
    public string Name;

    internal TreeClimbingState ClimbingState;

    internal TrieNode<ICombination, KeyEventCommand> CurrentNode => _trie.CurrentNode;

    internal bool IsOnRoot => _trie.IsOnRoot;
    internal TrieNode<ICombination, KeyEventCommand> Root => _trie.Root;
    private SequenceHotKeyStateResetter _stateResetter;
    public KeyStateTree(string name, IKeyTipNotifier notify, ILogger logger)
    {
        _notify = notify;
        _logger = logger;
        Name = name;
        _stateResetter = new SequenceHotKeyStateResetter(this);
    }

    public override string ToString()
    {
        return $"{{Tree:{Name}, State:{ClimbingState}, Node:{CurrentNode}}}";
    }

    public void Reset()
    {
        // _logger.LogInformation($"tree:{Name} reset, lastDownNodeForAllUp:{_lastKeyDownNodeForAllUp}");

        _lastKeyDownNodeForAllUp = null;
        _notify?.CloseKeysTip(Name);
        _trie.GoToRoot();
    }

    internal bool Contains(IHotkey hotKey)
    {
        var values = hotKey switch
        {
            ISequenceUnit k => _trie.TryGet([.. k.ToCombination()], out _),
            ISequence s => _trie.TryGet(s.ToList(), out _),
            _ => throw new Exception("not supported!")
        };

        return values;
    }

    internal void MarkDoneIfLanding()
    {
        if (ClimbingState == TreeClimbingState.LandingAndClimbOthers)
        {
            ClimbingState = TreeClimbingState.Done;
            //_logger.LogInformation($"Tree:{Name} State: Landing to Done");
        }
    }

    public IEnumerable<(string key, IEnumerable<string> descriptions)> Tips(bool ifRootThenEmpty = false)
    {
        if (ifRootThenEmpty && _trie.IsOnRoot)
        {
            return [];
        }

        return _trie.CurrentNode.Tip;
    }

    public IMetaKey Add(IList<ICombination> path, KeyEventCommand command)
    {
        if (TreeType == TreeType.SingleFruitPerEventType)
        {
            _trie.TryGet(path, out var node);
            if (node != null)
            {
                _trie.Remove(path, c => c.KeyEventType == command.KeyEventType);
            }
        }

        _trie.Add(path, command);
        return new MetaKey(_trie, path, command);
    }

    /// <summary>
    /// allUp is valid even if the trigger key is released before chord keys, so we record the last keyDown node for allUp processing versus the keyUp node
    /// </summary>
    private TrieNode<ICombination, KeyEventCommand>? _lastKeyDownNodeForAllUp = null;

    /// <summary>
    /// these chords are disabled, the key can not be used in the chord part of combination
    /// </summary>
    private readonly HashSet<Chord> _disabledChords = [];

    private readonly IKeyTipNotifier _notify;
    private readonly ILogger _logger;

    internal void DisableChord(Chord chord)
    {
        _disabledChords.Add(chord);
    }

    internal void EnableChord(Chord chord)
    {
        _disabledChords.Remove(chord);
    }

    /// <summary>
    /// only select candidate child node by its trigger key event
    /// with the key event(down or up), try to find the best matching child node from current node
    /// best matching: chord is not disabled in current tree, is not marked disabled in tree node,
    /// chord+trigger are all down, the one with most chord keys down.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>when return null, candidate node had value</returns>
    internal TrieNode<ICombination, KeyEventCommand>? TryFindChildNode(IKeyEventArgs args)
    {
        _stateResetter.Pulse();
        // to handle A+B+C(B is currently down in Chord)eventType == KeyEventType.Down && args.KeyCode == KeyCodes.RShiftKey/
        // for all children, if any is downInChord, we mark the state to continue
        var downInChord = false;
        ICombination? candidateKey = null;

        foreach (var childKey in _trie.CurrentNode.Children.Keys)
        {
            if (_disabledChords.Contains(childKey.Chord))
                continue;

            // mark down_in_chord and continue try to find trigger
            // currently no exact match:
            // other key not in chord down will still trigger: A+B+C, if D is down too.
            // question: should we just do exact match? means only exact chord match will trigger the hotkey
            if (args.IsKeyDown && childKey.Chord.Contains(args.KeyCode))
                downInChord = true;

            if (args.KeyCode != childKey.TriggerKey || childKey.Disabled)
                continue;
            // A+B and A+B+C, and B is down, so A+B will be selected
            // here: current's trigger key down
            var allChordDown = childKey.Chord.All(args.KeyboardState.IsDown);
            if (!allChordDown)
                continue;

            // here: current's all chord is down
            if (candidateKey == null)
            {
                candidateKey = childKey;
                continue;
            }
            // select the one with most chord keys down: A+B+C vs A+C -> A+B+C when A and B are down
            if (candidateKey.ChordCount < childKey.ChordCount)
            {
                candidateKey = childKey;
            }
        }

        if (candidateKey != null)
        {
            _trie.CurrentNode.Children.TryGetValue(candidateKey, out var candidate);
            if (args.IsKeyDown)
            {
                _lastKeyDownNodeForAllUp = candidate;
            }
            return candidate;
        }

        var candidateNode = ProcessTreeStateWhenNoCandidate(args, downInChord);
        return candidateNode;
    }

    internal TrieNode<ICombination, KeyEventCommand>? ProcessTreeStateWhenNoCandidate(IKeyEventArgs args, bool downInChord)
    {
        if (args.KeyEventType == KeyEventType.Down)
        {
            //  KeyInChord_down:(C+D, A+B) when A_down
            if (downInChord)
            {
                ClimbingState = TreeClimbingState.Continue_ChordDown_WaitForTrigger; // waiting for trigger key
                return null;
            }

            // down of trigger key
            if (_trie.IsOnRoot) // no child found and current node is root
            {
                // AnyKeyNotInRoot_down: i.e. *+A_now_down is not registered in root
                ClimbingState = TreeClimbingState.LandingAndClimbOthers;
                return null;
            }
            // on path, down of no trigger key, redo climbing
            Reset();
            ClimbingState = TreeClimbingState.LandingAndClimbAll;
            return null;
        }
        // up event when no candidateNode
        //
        // allUp design goal:
        // 1. could register allUp event
        // 2. still can trigger even when A+B+C_up has not triggered because of chord_up before trigger_up.
        if (_lastKeyDownNodeForAllUp?.Key.IsAnyKey(args.KeyCode) == true)
        {
            if (args.KeyboardState.AreAllUp(_lastKeyDownNodeForAllUp.Key.AllKeys))// trigger AllUp event
            {
                var candidateNode = _lastKeyDownNodeForAllUp;
                _lastKeyDownNodeForAllUp = null;
                args.KeyEventType = KeyEventType.AllUp;
                return candidateNode; // candidate set, so return null
            }
            // not all up:
            // A+C+B with C_up, wait for A_up. note: B_up will not arrive here, it will set the candidate when triggerKey(B_up) is up.
            ClimbingState = TreeClimbingState.Continue_ChordUp_WaitForTriggerOrOtherChordUp;
            return null;
        }
        //
        // the key_up is not any key in _lastKeyDownNode or _lasKeyDownNode is null
        //

        if (_trie.IsOnRoot)
        {
            // AnyKeyNotRegisteredInRoot_down_or_up: *A_down *A_up is not registered in root
            ClimbingState = TreeClimbingState.LandingAndClimbOthers;
            return null;
        }

        // on the path, no child, and up event
        if (_trie.CurrentNode.Children.Count == 0)
        {
            // NoChild & NotOnRoot:
            //   KeyInChord_up : A+B when A_up.
            //   other keyUp: A+B and B map to C??
            Reset();
            ClimbingState = TreeClimbingState.LandingAndClimbAll; // Chord_up would be processed on root
            return null;
        }

        // HaveChild & KeyInChord_up: A+B, C when B_up already set the currentNode, and then A_up continue wait C
        if (_trie.CurrentNode.Key.Chord.Contains(args.KeyCode))
        {
            ClimbingState = TreeClimbingState.Continue_ChordUp_TriggerAlreadyUp_WaitForChildKeys;
            return null;
        }

        //HaveChild & KeyNotInChord_up: B+D, F when C_up.
        Reset();
        ClimbingState = TreeClimbingState.LandingAndClimbAll;
        return null;
    }

    // climb to candidate node, execute actions
    internal void Climb(IKeyEventArgs args, TrieNode<ICombination, KeyEventCommand> candidateNode)
    {
        // conditional dbg:
        // Name == "ChordMap" && eventType == KeyEventType.Up && args.KeyCode == KeyCodes.Enter
        // Debug.Assert(args != null, nameof(args) + " != null");

        if (args.NoFurtherProcess)
        {
            ClimbingState = TreeClimbingState.NoFurtherProcess;
            return;
        }

        // no match
        // Chord_downOrUp? or
        // Debug.Assert(candidateNode != null, "candidateNode != null");

        var eventType = args.KeyEventType;
        var handled = candidateNode.Key.TriggerKey.Handled;
        if ((eventType != KeyEventType.AllUp) && (eventType & handled) != 0)
        {
            args.Handled = true; // even there is not action in list we still mark it as required,for all up
            _logger.LogInformation($"\t{eventType} event marked as Handled, tree: {Name}, candidate: {candidateNode.KeyPath}");
        }
        //

        // matched
        /// execute actions of down/up/allUp
        ExecuteActions(args, candidateNode, _logger, Name);

        //
        // goto candidateNode
        //
        GotoCandidate(args, candidateNode, eventType);

    }
    /// <summary>
    ///  return null if no exec
    /// </summary>
    static bool? ExecuteActions(IKeyEventArgs args, TrieNode<ICombination, KeyEventCommand> candidateNode, ILogger logger, string treeName)
    {
        bool? oneExecuted = null;
        var actionList = candidateNode.Values as KeyActionList<KeyEventCommand>;
        Debug.Assert(actionList != null, $"{nameof(actionList)} should be the type of KeyActionList<KeyEventCommand>");
        //KeyEventType[] eventTypes = [eventType];
        //if (eventType == KeyEventType.AllUp) eventTypes = [KeyEventType.Up, KeyEventType.AllUp];
        //foreach (var eventTyp in eventTypes)
        var eventTyp = args.KeyEventType;

        foreach (var keyCommand in actionList[eventTyp])
        {
            if (keyCommand.CanExecute != null && !keyCommand.CanExecute(args))
            {
                logger.LogWarning($"\tevent:{eventTyp},command({keyCommand.Description}) can not execute.(tree:{treeName}, node:{candidateNode})");
                oneExecuted ??= false;
                continue;
            }

            oneExecuted = true;
            var execute = keyCommand.Execute;

            var isAsync = execute?.Method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
            logger.LogInformation($"\tExecutedCommand:'{keyCommand.Description}',{(isAsync ? "Async," : "")} (tree:{treeName}, KeyPath:{candidateNode.KeyPath})"); // id:{keyCommand.Id}
            try
            {
                //if (args.KeyEventType == KeyEventType.Up) Debugger.Break();
                execute?.Invoke(args);
                if (args.NoFurtherProcess)
                {
                    break;
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.LogError(e, $"Error executing command:{keyCommand.Id} in tree:{treeName}");
            }
        }

        if (oneExecuted == null)
            logger.LogWarning($"\tAll event of type:{eventTyp} not executable!(tree:{treeName}, node:{candidateNode})");

        return oneExecuted;
    }

    private void GotoCandidate(IKeyEventArgs args, TrieNode<ICombination, KeyEventCommand> candidateNode, KeyEventType eventType)
    {
        if (args.PathToGo != null && !args.PathToGo.SequenceEqual(candidateNode.KeyPath)) // goto state by requiring
        {
            if (!_trie.TryGoTo(args.PathToGo.ToList(), out var state))
            {
                _logger.LogInformation($"Couldn't go to state {state}");
            }
            _lastKeyDownNodeForAllUp = null; // this force goto like the down climbing, so reset it for up keys monitoring

            ClimbingState = TreeClimbingState.Continue_AfterGoToPath;
            return;
        }

        switch (eventType)
        {
            case KeyEventType.Down:
                ClimbingState = TreeClimbingState.Continue_TriggerDown_WaitForUp;
                return;

            case KeyEventType.Up:
                // only navigate on up or AllUp event
                _trie.CurrentNode = candidateNode;

                if (candidateNode.Children.Count == 0)
                {
                    var actionList = (KeyActionList<KeyEventCommand>)candidateNode.Values;
                    if (actionList[KeyEventType.AllUp].Any())
                    {
                        // wait for chord up
                        ClimbingState = TreeClimbingState.Continue_TriggerUp_WaitForChordUpForAllUp;
                        return;
                    }

                    _notify?.CloseKeysTip(Name);
                    Reset();
                    ClimbingState = TreeClimbingState.Done;
                    return;
                }
                // has children
                _notify?.ShowKeysTip(Name, _trie.CurrentNode.Tip);
                // A, B: waiting for B
                ClimbingState = TreeClimbingState.Continue_TriggerUp_WaitForChildKeys;
                return;

            case KeyEventType.AllUp:
                _logger.LogInformation($"\tTree:{Name} run AllUp event, lastKeyDownNodeForAllUpEvent:{candidateNode}");

                // navigate on AllUp event only when not navigated by up
                if (_trie.CurrentNode.Equals(candidateNode)) // climbing had been done on KeyUp event
                {
                    // ClimbingState;
                    return;
                }
                // i.e. A+B down then A_up then B_up, climb to child node is not triggered by B_up, but by AllUp event
                _trie.CurrentNode = candidateNode;

                if (candidateNode.Children.Count == 0)
                {
                    Reset();
                    _notify?.CloseKeysTip(Name);
                    ClimbingState = TreeClimbingState.Done;
                    return;
                }

                _notify?.ShowKeysTip(Name, _trie.CurrentNode.Tip);
                ClimbingState = TreeClimbingState.Continue_AllUp_WaitForChildKeys;
                return;

            default:
                throw new Exception($"KeyEvent: {eventType} not supported");
        }
    }
}