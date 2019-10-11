using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Command;

namespace Metatool.Input
{
    public interface IKeyboardInternal
    {
        IKeyToken GetToken(ICommandToken<IKeyEventArgs> commandToken,
            IKeyboardCommandTrigger trigger);

        IToggleKey GeToggleKey(Key key);
    }
}
