using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.Internal;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

partial class Keyboard: IKeyboardInternal
{
	public IKeyCommand GetToken(ICommandToken<IKeyEventArgs> commandToken, IKeyboardCommandTrigger trigger) => new KeyCommandToken(commandToken, trigger);

	public IToggleKey GetToggleKey(Key key) => new ToggleKey(key);

	public void ReleaseDownKeys()
	{
		var downKeys = KeyboardState.CurrentAsync().AllDownKeys;
		foreach (var downKey in downKeys)
		{
			_logger.LogInformation($"Rest down key: {downKey}");
			Up((Key)downKey);
		}
	}
}