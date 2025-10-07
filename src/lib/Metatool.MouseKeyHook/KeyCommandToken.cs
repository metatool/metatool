using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public class KeyCommandToken : IKeyCommand
{
	private readonly ICommandToken<IKeyEventArgs> _internalCommandToken;
	private readonly IKeyboardCommandTrigger      _trigger;

	public KeyCommandToken(ICommandToken<IKeyEventArgs> internalCommandToken, IKeyboardCommandTrigger trigger)
	{
		_internalCommandToken = internalCommandToken;
		_trigger              = trigger;
	}

	public string Id
	{
		get => _trigger.MetaKey.Id;
		set => _trigger.MetaKey.Id = value;
	}

	public bool Change(IHotkey key)
	{
		return _trigger.MetaKey.ChangeHotkey(key) != null;
	}

	public void Remove()
	{
		_internalCommandToken.Remove();
	}

	public bool Change(ICommandTrigger<IKeyEventArgs> key)
	{
		return _internalCommandToken.Change(key);
	}

	internal IMetaKey metaKey => _trigger.MetaKey;
}
