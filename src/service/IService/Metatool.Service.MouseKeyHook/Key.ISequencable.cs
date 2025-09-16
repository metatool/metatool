namespace Metatool.Service.MouseKey;

public partial class Key
{
	public ISequence Then(KeyValues key) => new Combination(this).Then(key);

	public ISequence Then(IHotkey hotkey) => new Combination(this).Then(hotkey);

	public ISequence ToSequence() => this.ToCombination().ToSequence();

}