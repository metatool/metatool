using Metatool.Input;

namespace Metatool.Command{
    public interface IKey: ICommandToken<IKeyEventArgs>
    {
        bool Change(IHotkey key);
    }
}
