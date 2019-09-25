using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{
    public interface ICommandTrigger<TArgs>
    {
        void Execute(TArgs args);
        bool CanExecute(TArgs args);
        ICommand<TArgs> Command { get; set; }
        void OnRemove();
    }
}
