using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input
{
    public interface IKeyState: IEnumerable<ICombination>
    {
        bool Disabled { get; set; }
    }
}