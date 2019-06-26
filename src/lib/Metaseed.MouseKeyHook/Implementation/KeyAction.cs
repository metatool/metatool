using System;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    public class KeyAction
    {
        public readonly string ActionId;
        public readonly Action<KeyEventArgsExt> Action;
        public readonly string Description;

       public KeyAction(string actionId,string description, Action<KeyEventArgsExt> action)
        {
            ActionId = actionId;
            Action = action;
            Description = description;
        }

        public override string ToString()
        {
            return $"{ActionId}: {Description}";
        }
    }

    public class KeyEventAction
    {
        public KeyEvent KeyEvent { get; }
        public KeyAction Action { get; }

        public KeyEventAction(KeyEvent keyEvent, KeyAction action)
        {
            KeyEvent = keyEvent;
            Action = action;
        }
        public override int GetHashCode()
        {
            return (int)KeyEvent | Action?.GetHashCode()??0;
        }
        public override string ToString()
        {
            return KeyEvent + ": " + Action?.ActionId;
        }

        protected bool Equals(KeyEventAction other)
        {
            return KeyEvent == other.KeyEvent && Action == other.Action;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KeyEventAction)obj);
        }
    }
}
