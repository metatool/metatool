using System;

namespace Metatool.Service
{
    public interface ICommandTrigger<TArgs>
    {
        event Action<TArgs> Execute;
        event Predicate<TArgs> CanExecute;
        void OnAdd(ICommand<TArgs> command);
        void OnRemove(ICommand<TArgs> command);
    }
}
