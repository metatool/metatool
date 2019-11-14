using System.Collections.Generic;

namespace Metatool.Service
{
    public interface  IHotkey: ISequencable
    {
        ISequence ToSequence();
    }
}
