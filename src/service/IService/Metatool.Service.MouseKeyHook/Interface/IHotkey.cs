namespace Metatool.Service.MouseKey;
/// <summary>
/// there are 2 kinds of hotkey: ISequence, ISequenceUnit (Key or Combination)
/// </summary>
public interface  IHotkey: ISequencable
{
	ISequence ToSequence();
	KeyEventType Handled { get; set; }
}