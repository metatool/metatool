namespace Metatool.Service;

public interface ISequenceUnit: IHotkey
{
	ICombination ToCombination();
}