namespace Metatool.Service.MouseKey;

public interface ISequencable 
{
	ISequence Then(KeyValues key);
	ISequence Then(IHotkey hotkey);
}