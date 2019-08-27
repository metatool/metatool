using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using Metaseed.Input;
using Metaseed.ScreenHint;
using Metaseed.UI.Implementation;
using static Metaseed.MetaKeyboard.KeyboardConfig;
using static Metaseed.Input.Key;
using Window = Metaseed.UI.Window;

namespace Metaseed.MetaKeyboard
{
    public class Mouse : KeyMetaPackage
    {
        static readonly Hint Hint= new Hint();
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
                x = (int)(r.X + r.Width  / 2);
                y = (int)(r.Y + r.Height / 2);
            }

            FlaUI.Core.Input.Mouse.MoveTo(x, y);
        }

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseToFocus = (GK + F).Handled().Down(e => { MoveCursorToActiveControl(); });

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseScrollUp = (GK + W).Handled().Down(e => { Input.Mouse.VerticalScroll(1); });

        public IMetaKey MouseScrollDown = (GK + S).Handled().Down(e => { Input.Mouse.VerticalScroll(-1); });

        public IMetaKey MouseLeftClick = Hint.MouseClick.SetHotkey(Caps+S);
        public IMetaKey MouseLeftClick_Last = Hint.MouseClickLast.SetHotkey(Caps+A);

    }
}