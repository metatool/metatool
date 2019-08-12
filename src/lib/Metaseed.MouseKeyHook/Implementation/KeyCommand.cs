using System;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    public class KeyCommand : Command<KeyEventArgsExt>
    {
        public KeyCommand(Action<KeyEventArgsExt> action)
        {
            Execute = action;
        }
    }

    public class KeyEventCommand
    {
        public KeyEvent   KeyEvent { get; }
        public KeyCommand Command  { get; }

        public KeyEventCommand(KeyEvent keyEvent, KeyCommand command)
        {
            KeyEvent = keyEvent;
            Command  = command;
        }

        public override int GetHashCode()
        {
            return (int) KeyEvent | Command?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return KeyEvent + ": " + Command.Description;
        }

        protected bool Equals(KeyEventCommand other)
        {
            return KeyEvent == other.KeyEvent && Command == other.Command;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KeyEventCommand) obj);
        }
    }
}