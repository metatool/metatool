using Metatool.Input;

namespace Metatool.Command{
    public interface IKeyCommand: ICommandToken<IKeyEventArgs>
    {
        bool Change(IHotkey key);
    }
}
