using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{
    public interface ICommandTrigger<out TArgs>
    {
        event Action<TArgs> Execute;
        event Predicate<TArgs> CanExecute;
        void OnAdd(ICommand<TArgs> command);
        void OnRemove(ICommand<TArgs> command);
    }
}
