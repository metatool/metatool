using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input
{
    public interface ISequence : IEnumerable<ICombination>, ISequencable, IKeyPath
    {
    }
}
