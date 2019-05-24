using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Metaseed.Input;
using Metaseed.InputHook;

namespace Metaseed.Input
{
    public static class KeysExtensions
    {

        public static void Down(this Keys key, Action<KeyEventArgsExt> action)
        {
            KeyboardHook.Keys.Add((key,KeyEventType.Down), action);
        }

        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key,chord);
        }
    }
}