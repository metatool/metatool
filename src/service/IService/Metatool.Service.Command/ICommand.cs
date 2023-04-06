using System;

namespace Metatool.Service;

public interface ICommand
{
	string Description { get; set; }
	bool   Disabled     { get; set; }
}

public interface ICommand<in T> : ICommand
{
	Predicate<T> CanExecute { get; }
	Action<T>    Execute    { get; }
}