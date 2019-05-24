using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Metaseed.Input;

namespace Metaseed.Input
{
    public static class KeysExtensions
    {
        public static void Hit(this Keys key, Action<KeyEventArgsExt> action)
        {
            KeyboardHook.Combinations.Add(new Combination(key)._combination, action);
        }

        public static ICombination With(this Keys key, Keys chord)
        {
            return new Combination(key,chord);
        }
    }
}