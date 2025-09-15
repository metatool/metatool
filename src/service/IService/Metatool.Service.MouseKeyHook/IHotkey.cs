namespace Metatool.Service;

public interface  IHotkey: ISequencable
{
	ISequence ToSequence();
	KeyEventType Handled { get; set; }
}