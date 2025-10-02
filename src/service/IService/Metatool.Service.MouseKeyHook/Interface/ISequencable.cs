namespace Metatool.Service.MouseKey;

public interface ISequencable 
{
	ISequence Then(KeyCodes key);
	ISequence Then(IHotkey hotkey);
}