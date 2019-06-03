using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.Input;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class CombinationExtetions
    {
        public static void Down(this ICombination combination, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            Keyboard.Add(combination, new KeyAction(actionId,description,action));
        }
        
        public static void Up(this ICombination combination, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            Keyboard.Add(new Combination(combination.TriggerKey,combination.Chord,KeyEventType.Up), new KeyAction(actionId,description,action));
        }
    }
}
