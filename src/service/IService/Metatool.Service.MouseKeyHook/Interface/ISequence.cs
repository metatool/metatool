using System.Collections.Generic;

namespace Metatool.Service.MouseKey;

public interface ISequence : IList<ICombination>, IHotkey, IKeyPath
{
}