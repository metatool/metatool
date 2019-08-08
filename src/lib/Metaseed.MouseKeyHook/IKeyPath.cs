using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input
{
    public interface IKeyPath : IEnumerable<ICombination>
    {
        bool   Disabled { get; set; }
        object Context  { get; set; }
    }
}