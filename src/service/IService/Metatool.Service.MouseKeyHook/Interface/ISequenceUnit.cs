namespace Metatool.Service.MouseKey;

public interface ISequenceUnit: IHotkey
{
	ICombination ToCombination();
}