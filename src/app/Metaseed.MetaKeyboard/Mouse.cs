using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.ScreenHint;
using Metaseed.UI;
using static Metaseed.MetaKeyboard.KeyboardConfig;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class Mouse : KeyMetaPackage
    {
        // LButton & RButton
        public IMetaKey MouseLB = (GK + OpenBrackets).Map(Keys.LButton);
        public IMetaKey MouseRB = (GK + CloseBrackets).Map(Keys.RButton);

        static void MoveCursorToActiveControl()
        {
            using var automation = new UIA3Automation();
            var       active     = automation.FocusedElement();
            var       bounding   = active.BoundingRectangle;

            var x = bounding.X + bounding.Width  / 2;
            var y = bounding.Y + bounding.Height / 2;
            if (x == 0 && y == 0)
            {
                var r = Window.GetCurrentWindowRect();
                x = (int) (r.X + r.Width  / 2);
                y = (int) (r.Y + r.Height / 2);
            }

            FlaUI.Core.Input.Mouse.MoveTo(x, y);
        }

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseToFocus = (GK + F).Handled().Down(e => { MoveCursorToActiveControl(); });

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseScrollUp = (GK + W).Handled().Down(e => { Input.Mouse.VerticalScroll(1); });

        public IMetaKey MouseScrollDown = (GK + S).Handled().Down(e => { Input.Mouse.VerticalScroll(-1); });

        public IMetaKey MouseClick = (Caps + S).Down(e =>
        {
            e.Handled = true;
            e.BeginInvoke(() => Hint.Show());
        });

        public IMetaKey MouseClickLast = (Caps + A).Down(e =>
        {
            e.Handled = true;
            e.BeginInvoke(() => Hint.Show(false));
        });
    }
}