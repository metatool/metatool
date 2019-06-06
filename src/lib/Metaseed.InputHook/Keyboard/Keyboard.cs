using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WindowsInput.Native;
using Metaseed.Input.MouseKeyHook;
using Metaseed.Input.MouseKeyHook.Implementation;

namespace Metaseed.Input
{
    public static class Keyboard
    {
        static readonly KeyboardHook _Hook = new KeyboardHook();

        public static void Add(ICombination combination, KeyAction action)
        {
            _Hook.Add(combination,action);
        }

        public static void Add(IList<ICombination> combinations, KeyAction action)
        {
            _Hook.Add(combinations,action);
        }

        public static void HotKey(this string keys, string actionId, string description, Action action)
        {
            if (keys.Contains(','))
            {
                var sequence = Sequence.FromString(keys).ToList<ICombination>();
                Add(sequence, new KeyAction(actionId,description,e => action()) );
            }

            var combination =  Combination.FromString(keys) as Combination;
            Add(combination, new KeyAction(actionId, description, e => action()));
        }

        public static void Send(Keys key)
        {
            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)key);

        }

        public static void Hook()
        {
            _Hook.Run();
        }
    }
}
