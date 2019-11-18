using System;

namespace Metatool.Service
{
    public class CommandTrigger<TArgs> : ICommandTrigger<TArgs>
    {
        public event Action<TArgs>    Execute;
        public event Predicate<TArgs> CanExecute;

        public ICommandToken<TArgs> Register(ICommandManager commandManager, Action<TArgs> execute,
            Predicate<TArgs> canExecute = null, string description = "")
        {
            return commandManager.Add(this, execute, canExecute, description);
        }
        public virtual void OnExecute(TArgs args) => Execute?.Invoke(args);

        public virtual bool OnCanExecute(TArgs args) => CanExecute?.Invoke(args)??true;

        public virtual void OnAdd(ICommand<TArgs> command)
        {

        }
        public virtual void OnRemove(ICommand<TArgs> command)
        {
        }
    }
}
