using Metatool.Input;

namespace Metatool.Command{
    public interface IKey: ICommandToken<IKeyEventArgs>, IChange<IHotkey>
    {

    }
}
