using Metatool.Input;

namespace Metatool.Command{
    public interface IKeyToken: ICommandToken<IKeyEventArgs>, IChange<IHotkey>
    {

    }
}
