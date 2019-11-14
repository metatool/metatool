using System.Collections.Generic;

namespace Metatool.Service
{
    public interface ISequence : IList<ICombination>, IHotkey, IKeyPath
    {
    }
}
