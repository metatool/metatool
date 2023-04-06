using System;

namespace Metatool.Service;

public interface ICommandManager
{
	ICommandToken<T> Add<T>(ICommandTrigger<T> trigger, Action<T> execute,
		Predicate<T> canExecute = null, string description = "");


}