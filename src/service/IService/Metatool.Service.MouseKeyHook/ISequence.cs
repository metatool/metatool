using System.Collections.Generic;

namespace Metatool.Service
{
    public interface ISequence : IEnumerable<ICombination>, IHotkey, IKeyPath
    {
    }
}
