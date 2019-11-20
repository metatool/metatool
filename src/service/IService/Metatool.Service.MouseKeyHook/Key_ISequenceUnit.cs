namespace Metatool.Service
{
    public partial class Key
    {
        public ICombination ToCombination()
        {
            return (Combination) this;
        }
    }
}