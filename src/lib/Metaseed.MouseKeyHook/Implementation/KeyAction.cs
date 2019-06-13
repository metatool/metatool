using System;
using System.Collections.Generic;
using System.Text;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    public class KeyAction
    {
        public readonly string ActionId;
        public readonly Action<KeyEventArgsExt> Action;
        public readonly string Description;

        public KeyAction(string actionId,string description, Action<KeyEventArgsExt> action )
        {
            ActionId = actionId;
            Action = action;
            Description = description;
        }

        public override string ToString()
        {
            return ActionId + Description;
        }
    }
}
