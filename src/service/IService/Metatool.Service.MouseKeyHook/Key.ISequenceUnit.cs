namespace Metatool.Service.MouseKey;

public partial class Key
{
	public ICombination ToCombination()
	{
		return (Combination) this;
	}
}