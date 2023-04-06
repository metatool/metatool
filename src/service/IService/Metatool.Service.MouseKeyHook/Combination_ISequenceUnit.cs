namespace Metatool.Service;

public partial class Combination
{
	public ICombination ToCombination() => this;
	public KeyEvent Handled
	{
		get => this.TriggerKey.Handled;
		set => this.TriggerKey.Handled = value;
	}

}