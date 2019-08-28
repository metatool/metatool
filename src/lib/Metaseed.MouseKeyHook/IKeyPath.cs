using System.Collections.Generic;

namespace Metaseed.Input
{
    public interface IKeyPath : IEnumerable<ICombination>
    {
        bool   Disabled { get; set; }
        object Context  { get; set; }
    }
}