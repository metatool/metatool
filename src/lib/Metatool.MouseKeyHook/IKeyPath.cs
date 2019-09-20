using System.Collections.Generic;

namespace Metatool.Input
{
    public interface IKeyPath : IEnumerable<ICombination>
    {
        bool   Disabled { get; set; }
        object Context  { get; set; }
    }
}
