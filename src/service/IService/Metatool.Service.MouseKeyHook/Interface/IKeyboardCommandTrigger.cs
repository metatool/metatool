namespace Metatool.Service.MouseKey;

public interface IKeyboardCommandTrigger : ICommandTrigger<IKeyEventArgs>
{
	IMetaKey MetaKey { get; }
}