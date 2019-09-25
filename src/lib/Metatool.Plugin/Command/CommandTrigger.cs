using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{
    public class CommandTrigger<TArgs>:ICommandTrigger<TArgs>
    {
        public virtual ICommand<TArgs> Command { get; set; }

        public void Execute(TArgs args)
        {
            Command?.Execute(args);
        }

        public bool CanExecute(TArgs args)
        {
            var c=Command?.CanExecute;
            return c == null || c(args);
        }
        
        public virtual void OnRemove() { }

    }
}
