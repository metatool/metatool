using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{
    public class CommandTrigger<TArgs> : ICommandTrigger<TArgs>
    {
        public event Action<TArgs>    Execute;
        public event Predicate<TArgs> CanExecute;
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