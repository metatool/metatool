using System;
using System.Collections.Generic;
using System.Diagnostics;
using Metaseed;
using Metatool.Service;

namespace Metatool.Command;

public class CommandManager : ICommandManager
{
	[DebuggerDisplay("{ToString()}")]
	private class CommandEntry(object trigger, ICommand command)
    {
		public object Trigger { get; } = trigger;
        public ICommand Command { get; } = command;

        public override string ToString()
        {
            return $"Trigger:{Trigger}; Command:{Command}";
        }
    }

    public CommandManager()
    {
		DebugState.Add("CommandManager",this);
    }

	readonly Dictionary<ICommandToken, CommandEntry> _commands = new();

	private ICommandToken<TArgs> Add<TArgs>(ICommandTrigger<TArgs> trigger, ICommand<TArgs> command, ICommandToken<TArgs> token=null)
	{
		trigger.CanExecute += command.CanExecute;
		trigger.Execute    += command.Execute;
		trigger.OnAdd(command);
		token ??= new CommandToken<TArgs>(this);
		_commands.Add(token, new CommandEntry(trigger, command));
		return token;
	}

	public ICommandToken<TArgs> Add<TArgs>(ICommandTrigger<TArgs> trigger, Action<TArgs> execute,
		Predicate<TArgs> canExecute = null, string description = "")
	{
		var command = new Command<TArgs>() {Execute = execute, CanExecute = canExecute, Description = description};
		return Add(trigger, command);
	}

	public void DisableEnable<T>(ICommandToken<T> token, bool disable)
	{
		_commands[token].Command.Disabled = disable;
	}

	public bool IsDisabled<T>(ICommandToken<T> token)
	{
		return _commands[token].Command.Disabled;
	}

	public bool Change<T>(ICommandToken<T> token, ICommandTrigger<T> trigger)
	{
		var command = _commands[token].Command as ICommand<T>;
		Remove(token);
		Add(trigger, command, token);
		return true;
	}

	internal void Remove<T>(ICommandToken<T> token)
	{
		var command = _commands[token].Command as Command<T>;
		var trigger = _commands[token].Trigger as ICommandTrigger<T>;
		_commands.Remove(token);
		Debug.Assert(command != null, nameof(command) + " != null");
		Debug.Assert(trigger != null, nameof(trigger) + " != null");
		trigger.CanExecute -= command.CanExecute;
		command.CanExecute =  null;
		trigger.Execute    -= command.Execute;
		command.Execute    =  null;
		trigger.OnRemove(command);
	}
}