using System;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation;

public class KeyCommand : Command<IKeyEventArgs>
{
	public string Id { get; set; }

	public KeyCommand(Action<IKeyEventArgs> action)
	{
		Execute = action;
	}
}

public class KeyEventCommand
{
	public KeyEventType   KeyEventType { get; }
	public KeyCommand Command  { get; }

	public KeyEventCommand(KeyEventType keyEventType, KeyCommand command)
	{
		KeyEventType = keyEventType;
		Command  = command;
	}

	public override int GetHashCode()
	{
		return (int) KeyEventType | Command?.GetHashCode() ?? 0;
	}

	public override string ToString()
	{
		return KeyEventType + ": " + Command.Description;
	}

	protected bool Equals(KeyEventCommand other)
	{
		return KeyEventType == other.KeyEventType && Command == other.Command;
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((KeyEventCommand) obj);
	}
}