using System;
using System.Collections.Generic;
using Metatool.Command;
using Metatool.Core;
using OneOf;

namespace Metatool.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;



    public interface IChangeRemoveKey: IChangeRemove<Hotkey>
    {
    }

  
}
