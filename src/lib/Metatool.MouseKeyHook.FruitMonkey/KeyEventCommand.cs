using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation;

/// <summary>
/// values(fruits) stored in trie
/// </summary>
public class KeyEventCommand(KeyEventType keyEventType, KeyCommand command)
{
    /// <summary>
    /// why store KeyEventType in command? because the trie only has hotkeys.
    /// different commands can be triggered by different event types.
    /// </summary>
    public KeyEventType KeyEventType { get; } = keyEventType;
	public KeyCommand Command { get; } = command;

	public override int GetHashCode()
	{
		return (int)KeyEventType | Command?.GetHashCode() ?? 0;
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
		return Equals((KeyEventCommand)obj);
	}
}