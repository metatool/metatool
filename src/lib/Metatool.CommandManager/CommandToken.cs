using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Command
{

    public class CommandToken<T>: ICommandToken<T>
    {
        internal readonly CommandManager Manager;

        public CommandToken( ICommandManager manager)
        {
            Manager = manager as CommandManager;
        }

        public bool IsDisabled
        {
            set => Manager.DisableEnable(this, value);
            get => Manager.IsDisabled(this);
        }

        public bool Change(ICommandTrigger<T> trigger)
        {
            return Manager.Change(this, trigger);
        }

        public void Remove()
        {
            Manager.Remove(this);
        }
    }

    public class CommandTokens<T,TArg> :List<T>, ICommandToken<TArg> where T: ICommandToken<TArg>
    {
        public bool Change(ICommandTrigger<TArg> trigger)
        {
            this.ForEach(t => t.Change(trigger));
            return true;
        }

        public void Remove()
        {
            this.ForEach(t=>t.Remove());
            this.Clear();
        }
    }
}
