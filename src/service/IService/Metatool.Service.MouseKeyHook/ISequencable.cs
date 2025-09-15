namespace Metatool.Service;

public interface ISequencable 
{
	ISequence Then(KeyValues key);
	ISequence Then(IHotkey hotkey);
}