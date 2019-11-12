using Metatool.Command;

namespace Metatool.Service
{
    public interface IKeyboardInternal
    {
        IKeyCommand GetToken(ICommandToken<IKeyEventArgs> commandToken,
            IKeyboardCommandTrigger trigger);

        IToggleKey GeToggleKey(Key key);
    }
}
