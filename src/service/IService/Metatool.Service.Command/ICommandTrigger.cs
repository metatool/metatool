using System;

namespace Metatool.Service;

/// <summary>
/// trigger the execution of commands
/// </summary>
/// <typeparam name="TArgs"></typeparam>
public interface ICommandTrigger<out TArgs>
{
	event Action<TArgs> Execute;
	event Predicate<TArgs> CanExecute;
	void OnAdd(ICommand<TArgs> command);
	void OnRemove(ICommand<TArgs> command);
}