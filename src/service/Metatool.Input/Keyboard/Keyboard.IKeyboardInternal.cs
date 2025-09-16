using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

partial class Keyboard
{
	public IKeyCommand GetToken(ICommandToken<IKeyEventArgs> commandToken,
		IKeyboardCommandTrigger trigger) => new KeyCommandToken(commandToken, trigger);

	public IToggleKey GeToggleKey(Key key) => new ToggleKey(key);
	public void ReleaseDownKeys()
	{
		var downKeys = KeyboardState.Current().AllDownKeys;
		foreach (var downKey in downKeys)
		{
			Up((Key)downKey);
		}
	}
}