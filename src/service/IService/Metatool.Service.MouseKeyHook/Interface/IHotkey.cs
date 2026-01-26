namespace Metatool.Service.MouseKey;
/// <summary>
/// general trigger key: there are 2 kinds of hotkey: ISequence, ISequenceUnit (Key or Combination)
/// </summary>
public interface  IHotkey: ISequencable
{
	ISequence ToSequence();
	KeyEventType Handled { get; set; }
    /// <summary>
    /// Marks the hotkey as handled for the specified key event type and returns the current instance.
    /// </summary>
    /// <param name="eventType">The type of key event to mark as handled. The default is KeyEventType.All.</param>
    /// <returns>The current hotkey instance with the handled state set for the specified event type.</returns>
    IHotkey SetHandled(KeyEventType eventType = KeyEventType.All)
    {
        Handled = eventType;
        return this;
    }
}