using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Metatool.Command
{
    public interface ICommandManager
    {
        ICommandToken<T> Add<T>(ICommandTrigger<T> trigger, Action<T> execute,
            Predicate<T> canExecute = null, string description = "");

        void Remove<T>(ICommandTrigger<T> trigger);

    }
}
