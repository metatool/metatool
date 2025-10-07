using Metatool.Service.MouseKey;

namespace Metatool.Service.Internal;

public interface IKeyboardInternal
{
	IKeyCommand GetToken(ICommandToken<IKeyEventArgs> commandToken,
		IKeyboardCommandTrigger trigger);

	IToggleKey GetToggleKey(Key key);
}