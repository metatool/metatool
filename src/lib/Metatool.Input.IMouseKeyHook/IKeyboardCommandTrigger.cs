using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Command;
using Metatool.Input;

namespace Metatool.Command
{
    public interface IKeyboardCommandTrigger : ICommandTrigger<IKeyEventArgs>
    {
        IMetaKey MetaKey { get; }
    }
}
