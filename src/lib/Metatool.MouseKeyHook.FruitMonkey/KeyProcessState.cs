namespace Metatool.Input;

public enum KeyProcessState
{
    /// <summary>
    ///  well processed this event, and at root: event consumed, state reset
    /// </summary>
    Done,

    /// <summary>
    /// continue handling next event on the current state of the tree: event consumed, state kept
    /// </summary>
    Continue,

    /// <summary>
    /// reprocess this event on the root of trees, and the tree is at root: event reschedule(include the current event), state reset from path to root
    /// </summary>
    Reprocess,

    /// <summary>
    /// could not process the event, try to process this event with other trees at root. this event is dropped(exclude this event), reschedule next event, state reset
    /// </summary>
    Yield,

    /// <summary>
    /// stop further process for this event on any further tree: event consumed, state unknown
    /// </summary>
    NoFurtherProcess,
}
