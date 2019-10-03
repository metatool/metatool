using System.Collections.Generic;

namespace Metatool.Input
{
    public interface ISequence : IEnumerable<ICombination>, IHotkey, IKeyPath
    {
    }
}
