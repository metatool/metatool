using Metatool.Input;

namespace Metatool.Command{
    public interface IKeyboardCommandToken: ICommandToken<IKeyEventArgs>, IChange<IHotkey>
    {

    }
}
