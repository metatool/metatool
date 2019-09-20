using System.Windows.Forms;
using FlaUI.UIA3;
using Metatool.Input;
using Metatool.ScreenHint;
using static Metatool.MetaKeyboard.KeyboardConfig;
using static Metatool.Input.Key;
using Window = Metatool.UI.Window;

namespace Metatool.MetaKeyboard
{
    public class Mouse : KeyMetaPackage
    {
        // static readonly Hint Hint= new Hint();
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
            Input.Mouse.Simu.MoveToWithTrace(x, y);
        }

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseToFocus = (GK + F).Handled().Down(e => { e.BeginInvoke(MoveCursorToActiveControl); });

        // Scroll up/down (reading, one hand)
        public IMetaKey MouseScrollUp = (GK + W).Handled().Down(e => { Input.Mouse.Simu.VerticalScroll(1); });

        public IMetaKey MouseScrollDown = (GK + S).Handled().Down(e => { Input.Mouse.Simu.VerticalScroll(-1); });

        // public IMetaKey MouseLeftClick = Hint.MouseClick.ChangeHotkey(GK+C);
        // public IMetaKey MouseLeftClick_Last = Hint.MouseClickLast.ChangeHotkey(GK+LShift+C);

    }
}
