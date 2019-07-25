using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Metaseed.Input;

namespace Metaseed.MetaKeyboard
{
    public class Mouse
    {
        public Mouse()
        {
            // LButton & RButton
            Keys.OemOpenBrackets.With(Keys.CapsLock).Map(Keys.LButton);
            Keys.OemCloseBrackets.With(Keys.CapsLock).Map(Keys.RButton);

            // Scroll up/down (reading, one hand)
            Keys.E.With(Keys.CapsLock).Handled().Down(e => Input.Mouse.VerticalScroll(1));
            Keys.D.With(Keys.CapsLock).Handled().Down(e => Input.Mouse.VerticalScroll(-1));
        }
    }
}
