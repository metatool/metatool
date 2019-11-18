using System;

namespace Metatool.Service
{
    public interface ICommandTrigger<TArgs>
    {
        ICommandToken<TArgs> Register(ICommandManager commandManager, Action<TArgs> execute,
            Predicate<TArgs> canExecute = null, string description = "");
        event Action<TArgs> Execute;
        event Predicate<TArgs> CanExecute;
        void OnAdd(ICommand<TArgs> command);
        void OnRemove(ICommand<TArgs> command);
    }
}
