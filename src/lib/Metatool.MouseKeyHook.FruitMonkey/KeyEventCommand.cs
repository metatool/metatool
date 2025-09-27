using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation;

public class KeyEventCommand(KeyEventType keyEventType, KeyCommand command)
{
    public KeyEventType KeyEventType { get; } = keyEventType;
    public KeyCommand Command { get; } = command;

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