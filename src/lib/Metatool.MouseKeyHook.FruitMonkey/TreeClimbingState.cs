namespace Metatool.Input;

public enum TreeClimbingState
{
    /// <summary>
    ///  current tree is done, well processed this event, and at root: event consumed, state reset
    /// </summary>
    Done,

    /// <summary>
    /// wait and continue handling next event on the current state of the tree: event consumed, current node not changed
    /// i.e. A+B+C, B is down, C is not typed; or A,B: A is typed, waiting B
    /// i.e. another case: to waiting for up and All UP
    /// </summary>
    Continue_ChordDown_WaitForTrigger,
    Continue_TriggerDown_WaitForUp,
    Continue_TriggerUp_WaitForChordUpForAllUp,
    Continue_TriggerUp_WaitForChildKeys,
    Continue_ChordUp_WaitForTriggerOrOtherChordUp,
    Continue_ChordUp_TriggerAlreadyUp_WaitForChildKeys,
    Continue_AllUp_WaitForChildKeys,
    Continue_AfterGoToPath,
    /// <summary>
    /// current tree is done, reprocess this event on the root of trees, and the tree is at root: event reschedule(include the current event), state reset from path node to root
    /// vs Landing, current tree will process this even again too.
    /// </summary>
    LandingAndClimbAll,

    /// <summary>
    /// current tree is done, could not process the event, try to process this event with other trees at root. this event is dropped(exclude this event) for processing on current tree,
    /// reschedule next event, state reset
    /// </summary>
    LandingAndClimbOthers,

    /// <summary>
    /// current tree is done, stop further process for this event on any further tree: event consumed, state unknown
    /// </summary>
    NoFurtherProcess,
}
