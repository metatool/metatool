using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    public class KeyAction
    {
        public readonly string ActionId;
        public readonly Action<EventArgs> Action;
        public readonly string Description;

        public KeyAction(string actionId,string description, Action<EventArgs> action)
        {
            ActionId = actionId;
            Action = action;
            Description = description;
        }
    }
}
