using System;
using System.Windows;
using System.Windows.Automation;
using Metatool.Service;
using Point = System.Drawing.Point;

namespace Metatool.MetaKeyboard
{
    public class KeyboardMouse : CommandPackage
    {
        private readonly IMouse         _mouse;
        private static   IWindowManager _windowManager;

        public KeyboardMouse(IScreenHint screenHint, IMouse mouse, IWindowManager windowManager, IKeyboard keyboard, IConfig<Config> config)
        {
            _windowManager = windowManager;
            _mouse = mouse;

            RegisterCommands();
            var conf = config.CurrentValue;
            var maps = conf.KeyboardMousePackage.KeyMaps;
            keyboard.RegisterKeyMaps(maps);

            var hotkeys = conf.KeyboardMousePackage.HotKeys;
            hotkeys.MouseToFocus.Event(e => e.BeginInvoke(MoveCursorToActiveControl));
            hotkeys.MouseScrollUp.Event(e =>
            {
                var mouse = Services.Get<IMouse>();
                mouse.VerticalScroll(1);
            });
            hotkeys.MouseScrollDown.Event(e =>
            {
                var mouse = Services.Get<IMouse>();
                mouse.VerticalScroll(-1);
            });
            hotkeys.MouseLeftClick.Event(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => screenHint.Show(DoMouseLeftClick));
            });
            hotkeys.MouseLeftClickLast.Event(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => screenHint.Show(DoMouseLeftClick, false));
            });
        }


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