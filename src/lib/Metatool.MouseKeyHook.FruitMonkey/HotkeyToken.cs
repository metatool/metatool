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
