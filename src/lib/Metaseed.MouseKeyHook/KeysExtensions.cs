using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Metaseed.Input;
using Metaseed.Input.MouseKeyHook;

namespace Metaseed.Input
{
    public static class KeysExtensions
    {

        public static void Down(this Keys key, Action<KeyEventArgsExt> action)
        {
            KeyboardHook.KeyDown.Add(key, action);
        }

        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key,chord);
        }
        public static ICombination With(this Keys triggerKey, IEnumerable<Keys> chordsKeys)
        {
            return new Combination(triggerKey,chordsKeys);
        }
    }
}