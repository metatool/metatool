using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public class MetaKey(ITrie<ICombination, KeyEventCommand> trie, IList<ICombination> path, KeyEventCommand command) : IMetaKey
{
	internal readonly HotkeyToken _token = new HotkeyToken(trie, path, command);

    public IHotkey Hotkey
	{
		get
		{
			if (_token._hotkey.Count > 1) 
				return new Sequence(_token._hotkey.ToArray());

			var first = _token._hotkey.First();
			if (first.ChordLength > 0) 
				return first as Combination;

			return first.TriggerKey;
		}
		set => _token.Change(value);
	}

	public IMetaKey ChangeHotkey(IHotkey hotkey)
	{
		Hotkey = hotkey;
		return this;
	}

	public void Remove()
	{
		_token.Remove();
	}

	internal KeyEventType KeyEventType => _token.EventCommand.KeyEventType;

	public void ChangeDescription(string description)
	{
		_token.ChangeDescription(description);
	}

	public string Id
	{
		get => _token.EventCommand.Command.Id;
		set => _token.EventCommand.Command.Id = value;
	}

	public bool Disable
	{
		get => _token._hotkey.Last().Disabled;
		set => _token._hotkey.Last().Disabled = value;
	}
}
