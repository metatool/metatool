namespace Metatool.Input;

public enum TreeClimbingState
{
    /// <summary>
    ///  current tree is done, well processed this event, and at root: event consumed, state reset
    /// </summary>
    Done,

    /// <summary>
    /// wait and continue handling next event on the current state of the tree: event consumed, current node not changed
    /// i.e. A+B+C, B is down, C is not typed
    /// </summary>
    Continue,

    /// <summary>
    /// current tree is done, reprocess this event on the root of trees, and the tree is at root: event reschedule(include the current event), state reset from path node to root
    /// </summary>
    LandingAndClimbing,

    /// <summary>
    /// current tree is done, could not process the event, try to process this event with other trees at root. this event is dropped(exclude this event) for processing on current tree, reschedule next event, state reset
    /// </summary>
    Landing,

    /// <summary>
    /// current tree is done, stop further process for this event on any further tree: event consumed, state unknown
    /// </summary>
    NoFurtherProcess,
}
