using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Metatool.Service;

namespace Metatool.Command;

[DebuggerDisplay("{ToString()}")]
public class CommandToken<T>(ICommandManager manager) : ICommandToken<T>
{
	internal readonly CommandManager Manager = manager as CommandManager;

    public string Id { get; set; }

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

    public override string ToString()
    {
        return $"Id:{Id},Disabled:{IsDisabled}";
    }
}

public class CommandTokens<T, TArg> : List<T>, ICommandToken<TArg> where T : ICommandToken<TArg>
{
	public string Id
	{
		get => this.Aggregate("", (a, c) => a + c.Id);
		set
		{
			for (var i = 0; i < this.Count; i++)
			{
				var k = this[i];
				k.Id = value;
			}
		}
	}

	public bool Change(ICommandTrigger<TArg> trigger)
	{
		this.ForEach(t => t.Change(trigger));
		return true;
	}

	public void Remove()
	{
		this.ForEach(t => t.Remove());
		this.Clear();
	}
}