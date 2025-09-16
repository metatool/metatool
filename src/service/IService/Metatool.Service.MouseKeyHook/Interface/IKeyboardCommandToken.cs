namespace Metatool.Service.MouseKey;

public interface IKeyCommand: ICommandToken<IKeyEventArgs>
{
	bool Change(IHotkey key);
}