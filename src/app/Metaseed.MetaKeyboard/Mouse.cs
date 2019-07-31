using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Metaseed.Input;
using static Metaseed.MetaKeyboard.KeyboardConfig;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class Mouse
    {
        public Mouse()
        {
            // LButton & RButton
            (GK + OpenBrackets).Map(Keys.LButton);
            (GK + CloseBrackets).Map(Keys.RButton);

            // Scroll up/down (reading, one hand)
            (GK + E).Handled().Down(e => Input.Mouse.VerticalScroll(1));
            (GK + D).Handled().Down(e => Input.Mouse.VerticalScroll(-1));
        }
    }
}