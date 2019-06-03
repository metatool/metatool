using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class KeysExtensions
    {
        public static void Down(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            Keyboard.Add(new Combination(key), new KeyAction(actionId, description, action));
        }

        public static void Up(this Keys key, string actionId, string description, Action<KeyEventArgsExt> action)
        {
            Keyboard.Add(new Combination(key, KeyEventType.Up), new KeyAction(actionId, description, action));
        }


        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key, chord);
        }

        public static ICombination With(this Keys triggerKey, IEnumerable<Keys> chordsKeys)
        {
            return new Combination(triggerKey, chordsKeys);
        }
    }
}