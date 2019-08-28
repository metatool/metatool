using System.Collections.Generic;

namespace Metaseed.Input
{
    public interface ISequence : IEnumerable<ICombination>, ISequencable, IKeyPath
    {
    }
}
