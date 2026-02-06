using System;
using System.Collections.Generic;

namespace Metatool.Service;

public interface IRemove
{
	void Remove();
}

public interface IChange<in T>
{
	bool Change(T key);
}

public interface IChangeRemove<in T> : IRemove, IChange<T>
{
}

public class Removable : IRemove
{
	private readonly Action _removeAction;

	public Removable(Action removeAction)
	{
		_removeAction = removeAction;
	}

	public void Remove()
	{
		_removeAction();
	}
}

public class Removables : List<IRemove>, IRemove
{
	public void Remove()
	{
		ForEach(d => d.Remove());
	}
}

public class ChangeRemove<T> : List<IChangeRemove<T>>, IChangeRemove<T>
{
	public void Remove()
	{
		ForEach(d => d.Remove());
	}

	public bool Change(T key)
	{
		ForEach(d => d.Change(key));
		return true;
	}
}