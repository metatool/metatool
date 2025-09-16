namespace Metatool.Service.MouseKey;

public interface  IHotkey: ISequencable
{
	ISequence ToSequence();
	KeyEventType Handled { get; set; }
}