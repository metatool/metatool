using System.Linq;

namespace Metatool.Service.MouseKey;

public partial class Sequence: ISequencable
{
	public ISequence Then(KeyCodes key)
	{
		return this.Then(new Combination(key));
	}

	public ISequence Then(IHotkey hotkey)
	{
		this.AddRange(hotkey.ToSequence());
		return this;
	}

	public KeyEventType Handled
	{
		get => this.Last().TriggerKey.Handled;
		set => this.Last().TriggerKey.Handled = value;
	}
	public ISequence ToSequence() => this;
}