namespace Metatool.Service.MouseKey;

/// <summary>
/// there are 2 kinds of sequence unit: Key and Combination
/// </summary>
public interface ISequenceUnit: IHotkey
{
	ICombination ToCombination();
}