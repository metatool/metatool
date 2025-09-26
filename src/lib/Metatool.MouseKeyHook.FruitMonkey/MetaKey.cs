using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Trie;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input;

public class HotkeyToken : IChangeRemove<IHotkey>
{
	private readonly  ITrie<ICombination, KeyEventCommand> _trie;
	internal readonly IList<ICombination>                  _hotkey;
	internal readonly KeyEventCommand                      EventCommand;

	public HotkeyToken(ITrie<ICombination, KeyEventCommand> trie, IList<ICombination> hotkey,
		KeyEventCommand eventCommand)
	{
		_trie   = trie;
		_hotkey = hotkey;
		EventCommand = eventCommand;
	}

	public void Remove()
	{
		var r = _trie.Remove(_hotkey, eventCommand => eventCommand.Equals(EventCommand));
		Console.WriteLine(r);
	}

	public bool Change(IHotkey keyProperty)
	{
		((IRemove) this).Remove();
		switch(keyProperty)
		{
			case ISequenceUnit k:
				_trie.Add([k.ToCombination()], EventCommand);
				break;
			case ISequence s:
				_trie.Add(s.ToList(), EventCommand);
				break;
			default:throw new Exception("not supported!");
		}
		return true;
	}

	public void ChangeDescription(string description)
	{
		EventCommand.Command.Description = description;
	}
}

public class MetaKey : IMetaKey
{
	internal readonly HotkeyToken _token;

	public IHotkey Hotkey
	{
		get
		{
			if (_token._hotkey.Count > 1) return new Sequence(_token._hotkey.ToArray());
			var first = _token._hotkey.First();
			if (first.ChordLength > 0) return (first as Combination);

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

	public MetaKey(ITrie<ICombination, KeyEventCommand> trie, IList<ICombination> combinations,
		KeyEventCommand command)
	{
		_token = new HotkeyToken(trie, combinations, command);
	}
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

public class MetaKeys : List<IMetaKey>, IMetaKey
{
	public MetaKeys(List<IMetaKey> keys):base(keys)
	{
	}
	public string Id
	{
		get => this.Aggregate("", (a, c) => a + c.Id);
		set
		{
			for (var i = 0; i < this.Count; i++)
			{
				var k = (MetaKey) this[i];
				k.Id = value; //$"{value}_{i}-{k.KeyEvent}"
			}
		}
	}

	public bool Disable
	{
		get => this.First().Disable;
		set => this.ForEach(k => k.Disable = value);
	}

	public IHotkey Hotkey
	{
		get => this.First().Hotkey;
		set => this.ForEach(k => k.Hotkey = value);
	}

	public IMetaKey ChangeHotkey(IHotkey hotkey)
	{
		Hotkey = hotkey;
		return this;
	}

	public void ChangeDescription(string description)
	{
		this.ForEach(k =>k.ChangeDescription(description));
	}

	public void Remove()
	{
		this.ForEach(k => k.Remove());
	}
}