using Metatool.Input;

namespace Metatool.Command
{
    public interface IKeyboardCommandTrigger : ICommandTrigger<IKeyEventArgs>
    {
        IMetaKey MetaKey { get; }
    }
}
