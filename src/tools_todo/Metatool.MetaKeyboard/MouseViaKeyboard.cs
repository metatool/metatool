using System;
using System.Windows;
using System.Windows.Automation;
using Metatool.Command;
using static Metatool.MetaKeyboard.KeyboardConfig;
using static Metatool.Service.Key;
using Metatool.Service;
using Point = System.Drawing.Point;

namespace Metatool.MetaKeyboard
{
    public class MouseViaKeyboard : CommandPackage
    {
        private readonly IMouse         _mouse;
        private static   IWindowManager _windowManager;

        public MouseViaKeyboard(IScreenHint screenHint, IMouse mouse, IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _mouse = mouse;
            MouseLeftClick = (GK + C).Down(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => screenHint.Show(DoMouseLeftClick));
            });
            MouseLeftClickLast = (GK + LShift + C).Down(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => screenHint.Show(DoMouseLeftClick, false));
            });
            RegisterCommands();
        }

        // static readonly Hint Hint= new Hint();
        // LButton & RButton
        public IKeyCommand MouseLB = (GK + OpenBrackets).Map(LButton);
        public IKeyCommand MouseRB = (GK + CloseBrackets).Map(RButton);

        static void MoveCursorToActiveControl()
        {
            var active   = AutomationElement.FocusedElement;
            var bounding = (Rect) active.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);

            var x = (int) Math.Floor(bounding.X + bounding.Width  / 2);
            var y = (int) Math.Floor(bounding.Y + bounding.Height / 2);
            if (x == 0 && y == 0)
            {
                var r = _windowManager.CurrentWindow.Rect;
                x = (int) (r.X + r.Width  / 2);
                y = (int) (r.Y + r.Height / 2);
            }

            var mouse = Services.Get<IMouse>();
            mouse.MoveToLikeUser(x, y);
        }

        // Scroll up/down (reading, one hand)
        public IKeyCommand MouseToFocus = (GK + F).Handled().Down(e => { e.BeginInvoke(MoveCursorToActiveControl); });

        // Scroll up/down (reading, one hand)
        public IKeyCommand MouseScrollUp = (GK + W).Handled().Down(e =>
        {
            var mouse = Services.Get<IMouse>();
            mouse.VerticalScroll(1);
        });

        public IKeyCommand MouseScrollDown = (GK + S).Handled().Down(e =>
        {
            var mouse = Services.Get<IMouse>();
            mouse.VerticalScroll(-1);
        });

        public IKeyCommand MouseLeftClick;
        public IKeyCommand MouseLeftClickLast;

        void DoMouseLeftClick((Rect winRect, Rect clientRect) position)
        {
            var rect    = position.clientRect;
            var winRect = position.winRect;
            rect.X = winRect.X + rect.X;
            rect.Y = winRect.Y + rect.Y;
            var p = new Point((int) (rect.X + rect.Width / 2), (int) (rect.Y + rect.Height / 2));
            _mouse.Position = p;
            _mouse.LeftClick();
        }
    }
}