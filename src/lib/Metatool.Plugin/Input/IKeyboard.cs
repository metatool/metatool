using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Metatool.Command;

namespace Metatool.Input
{
    public interface IKeyboard
    {
        ICommandTrigger<IKeyEventArgs> Down(ISequenceUnit sequenceUnit);
        ICommandTrigger<IKeyEventArgs> Up(ISequenceUnit sequenceUnit);

    }
}
