using System;

namespace Metatool.Service;

public class Command<T> : ICommand<T>
{
	public Predicate<T> CanExecute { get; set; }
        
	public Action<T> Execute     { get; set; }
	public string    Description { get; set; }
	public bool      Disabled     { get; set; }

	public override string ToString()
	{
		return Description;
	}
}