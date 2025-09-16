namespace Metatool.Service.MouseKey;

public partial class Combination
{
	public ICombination ToCombination() => this;
	public KeyEventType Handled
	{
		get => this.TriggerKey.Handled;
		set => this.TriggerKey.Handled = value;
	}

}