namespace Metatool.Service.MouseKey;

public partial class Combination
{
	public ISequence Then(IHotkey hotkey)
	{
		var sequence = new Sequence(this);
		sequence.AddRange(hotkey.ToSequence());
		return sequence;
	}

	public ISequence Then(KeyCodes key)
	{
		return this.Then(new Combination(key));
	}
	public ISequence ToSequence() => new Sequence(this);
}