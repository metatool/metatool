namespace Metatool.Service;

public interface IKeyCommand: ICommandToken<IKeyEventArgs>
{
	bool Change(IHotkey key);
}