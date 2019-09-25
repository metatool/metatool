using System;
using System.Collections.Generic;

namespace Metatool.Command
{
    public class CommandManager:ICommandManager
    {
        readonly Dictionary<object,ICommand> _commands = new Dictionary<object, ICommand>();

        public ICommandToken<TArgs> Add<TArgs>(ICommandTrigger<TArgs> trigger, Action<TArgs> execute,
            Predicate<TArgs> canExecute = null, string description = "")
        {
            var command = new Command<TArgs>(){Execute = execute, CanExecute = canExecute, Description = description};
            trigger.Command = command;
            var token = new CommandToken<TArgs>(trigger, this);
            _commands.Add(trigger, command);
            return token;
        }

        public void EnableDisable<T>(ICommandTrigger<T> token, bool enable)
        {
            _commands[token].Enabled = enable;
        }

        public bool IsEnabled<T>(ICommandTrigger<T> token)
        {
            return _commands[token].Enabled;
        }

        public void Remove<T>(ICommandTrigger<T> trigger)
        {
            _commands.Remove(trigger);
            trigger.Command = null;

        }

    }
}
