using System;
using System.Collections.Generic;
using Metatool.Core;
using Metatool.Input.MouseKeyHook.Implementation.Command;
using OneOf;

namespace Metatool.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;



    public interface IChangeRemoveKey: IChangeRemove<Hotkey>
    {
    }

  
}
