using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Metaseed.Input;
using static Metaseed.MetaKeyboard.KeyboardConfig;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class Mouse : KeyMetaPackage
    {
        // LButton & RButton
        public IMetaKey MouseLB = (GK + OpenBrackets).Map(Keys.LButton);
        public IMetaKey MouseRB = (GK + CloseBrackets).Map(Keys.RButton);

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseScrollUp   = (GK + E).Handled().Down(e => Input.Mouse.VerticalScroll(1));
        public IMetaKey MouseScrollDown = (GK + D).Handled().Down(e => Input.Mouse.VerticalScroll(-1));
    }
}