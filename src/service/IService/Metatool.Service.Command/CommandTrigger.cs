using System;
using System.Diagnostics;

namespace Metatool.Service;

[DebuggerDisplay("{ToString()}")]
public class CommandTrigger<TArgs> : ICommandTrigger<TArgs>
{
	public event Action<TArgs>    Execute;
	public event Predicate<TArgs> CanExecute;
    protected int ExecuteCount => Execute?.GetInvocationList().Length ?? 0;
    protected int CanExecuteCount => CanExecute?.GetInvocationList().Length ?? 0;

    public virtual void OnExecute(TArgs args) => Execute?.Invoke(args);

	public virtual bool OnCanExecute(TArgs args) => CanExecute?.Invoke(args)??true;

	public virtual void OnAdd(ICommand<TArgs> command)
	{

    }
	public virtual void OnRemove(ICommand<TArgs> command)
	{
	}

    public override string ToString()
    {
        return $"Execute:{ExecuteCount},CanExecute:{CanExecuteCount}";
    }
}