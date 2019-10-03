using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Input;

namespace Metatool.Command{
    public interface IKeyboardCommandToken: ICommandToken<IKeyEventArgs>, IChange<IHotkey>
    {

    }
}
