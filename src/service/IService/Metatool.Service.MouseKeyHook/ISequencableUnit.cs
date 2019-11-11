namespace Metatool.Input
{
    public interface ISequenceUnit: IHotkey
    {
        ICombination ToCombination();
    }
}
