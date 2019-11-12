using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Metatool.Service;
using Metatool.WindowsInput.Native;

namespace Metatool.Input
{
    public partial class Keyboard
    {
        public void Type(Key key)
        {
            InputSimu.Inst.Keyboard.KeyPress((VirtualKeyCode)(Keys)key);
        }

        public void Type(Key[] keys) => InputSimu.Inst.Keyboard.KeyPress(keys.Select(k=>(VirtualKeyCode)(Keys)k).ToArray());

        public void Type(char character) => InputSimu.Inst.Keyboard.Type(character);

        public void Type(string text) => InputSimu.Inst.Keyboard.Type(text);
    }
}
