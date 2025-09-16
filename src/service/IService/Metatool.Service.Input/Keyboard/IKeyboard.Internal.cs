using Metatool.Service.MouseKey;

namespace Metatool.Service.Internal;

public interface IKeyboardInternal
{
	IKeyCommand GetToken(ICommandToken<IKeyEventArgs> commandToken,
		IKeyboardCommandTrigger trigger);

	IToggleKey GeToggleKey(Key key);
}