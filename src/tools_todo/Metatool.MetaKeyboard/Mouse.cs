using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
// using FlaUI.UIA3;
using Metatool.Command;
using Metatool.Input;
using static Metatool.MetaKeyboard.KeyboardConfig;
using static Metatool.Input.Key;
using Window = Metatool.Utils.Window;
using Metatool.Plugin;

namespace Metatool.MetaKeyboard
{
    public class Mouse : CommandPackage
    {
        // static readonly Hint Hint= new Hint();
        // LButton & RButton
        public IKey MouseLB = (GK + OpenBrackets).Map(Keys.LButton);
        public IKey MouseRB = (GK + CloseBrackets).Map(Keys.RButton);

        static void MoveCursorToActiveControl()
        {
            var active = AutomationElement.FocusedElement;
            var bounding = (Rect) active.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);

            var x = (int)Math.Floor(bounding.X + bounding.Width  / 2);
            var y = (int)Math.Floor(bounding.Y + bounding.Height / 2);
            if (x ==0 && y ==0)
            {
                var r = Window.GetCurrentWindowRect();
                x = (int) (r.X + r.Width  / 2);
                y = (int) (r.Y + r.Height / 2);
            }

            var mouse = Services.Get<IMouse>();
            mouse.MoveToLikeUser(x, y);
        }

        // Scroll up/down (reading, one hand)
        public IKey MouseToFocus = (GK + F).Handled().Down(e =>
        {
            e.BeginInvoke(MoveCursorToActiveControl);
        });

        // Scroll up/down (reading, one hand)
        public IKey MouseScrollUp = (GK + W).Handled().Down(e =>
        {
            var mouse = Services.Get<IMouse>();
            mouse.VerticalScroll(1);
        });

        public IKey MouseScrollDown = (GK + S).Handled().Down(e =>
        {
            var mouse = Services.Get<IMouse>();
            mouse.VerticalScroll(-1);
        });

        // public IKeyboardCommandToken  MouseLeftClick = Hint.MouseClick.ChangeHotkey(GK+C);
        // public IKeyboardCommandToken  MouseLeftClick_Last = Hint.MouseClickLast.ChangeHotkey(GK+LShift+C);
    }
}