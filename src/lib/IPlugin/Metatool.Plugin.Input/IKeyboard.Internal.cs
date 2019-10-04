using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Command;

namespace Metatool.Input
{
    public interface IKeyboardInternal
    {
        IKeyboardCommandToken GetToken(ICommandToken<IKeyEventArgs> commandToken,
            IKeyboardCommandTrigger trigger);
    }
}
