using Metatool.Service;
using Metatool.Tools.KeyMouse;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Point = System.Drawing.Point;

namespace Metatool.MetaKeyboard
{
    public class KeyboardMouseToolPackage : CommandPackage
    {
        public KeyboardMouseToolPackage(IScreenHint screenHint, IMouse mouse, IWindowManager windowManager, IKeyboard keyboard,
            IConfig<PluginConfig> config)
        {
            RegisterCommands();
            var conf = config.CurrentValue;
            screenHint.HintKeys = conf.KeyboardMousePackage.HintKeys;

            var maps = conf.KeyboardMousePackage.KeyMaps;
            keyboard.RegisterKeyMaps(maps);

            var hotkeys = conf.KeyboardMousePackage.Hotkeys;

            hotkeys.MouseToFocus.OnEvent(_ => MoveCursorToActiveWindow());

            hotkeys.MouseScrollUp.OnEvent(e =>
            {
                var mouse = Services.Get<IMouse>();
                mouse.VerticalScroll(1);
            });

            hotkeys.MouseScrollDown.OnEvent(e =>
            {
                mouse.VerticalScroll(-1);
            });

            hotkeys.MouseLeftClick.OnEvent(e =>
            {
                e.Handled = true;
                screenHint.Show(DoMouseLeftClick, activeWindowOnly: true, useWpfDetector: true);
            });

            hotkeys.MouseLeftClickAlt.OnEvent(e =>
            {
                e.Handled = true;
                screenHint.Show(DoMouseLeftClick, useWpfDetector: false);
            });

            hotkeys.MouseLeftClickLast.OnEvent(e =>
            {
                e.Handled = true;
                screenHint.Show(DoMouseLeftClick, false);
            });

            if (conf.KeyboardMousePackage.MouseFollowActiveWindow)
            {
                void ActiveWindowChanged(object o, IntPtr hwnd)
                {
                    var r = windowManager.CurrentWindow.Rect;
                    var x = (int)(r.X + r.Width / 2);
                    var y = (int)(r.Y + r.Height / 2);
                    if (x != 0 && y != 0)
                        mouse.Position = new Point(x, y);

                }
                windowManager.ActiveWindowChanged += ActiveWindowChanged;
            }

            void DoMouseLeftClick((IUIElement winRect, IUIElement clientRect) position)
            {
                var rect = position.clientRect;
                var winRect = position.winRect;
                var X = winRect.X + rect.X;
                var Y = winRect.Y + rect.Y;
                var p = new Point(X + rect.Width / 2, Y + rect.Height / 2);
                mouse.Position = p;
                mouse.LeftClick();
            }

            void MoveCursorToActiveWindow()
            {
                var r = windowManager.CurrentWindow.Rect;
                var x = (int)(r.X + r.Width / 2);
                var y = (int)(r.Y + r.Height / 2);
                if (x != 0 && y != 0)
                    Task.Run(() => mouse.MoveToLikeUser(x, y));
                // mouse.Position = new Point(x, y);
            }

            // moving to active control works, but I want to move to the scrollable control in the window not use it for now
            void MoveCursorToActiveControl()
            {
                var active = AutomationElement.FocusedElement;
                var bounding = (Rect)active.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                var x = (int)Math.Floor(bounding.X + bounding.Width / 2);
                var y = (int)Math.Floor(bounding.Y + bounding.Height / 2);
                var isElementInValid = !double.IsFinite(bounding.X) || !double.IsFinite(bounding.Y) || x == 0 && y == 0;
                if (!isElementInValid)
                {

                    var r = windowManager.CurrentWindow.Rect;
                    x = (int)(r.X + r.Width / 2);
                    y = (int)(r.Y + r.Height / 2);
                }
                // we need to run in task to avoid blocking keyboard hook thread, otherwise the keyboard event(i.e. F_up) may be lost, then a 'F' is typed even we mark the 'F_down' as handled.
                Task.Run(() => mouse.MoveToLikeUser(x, y));
            }
        }

        public void OnUnloading()
        {
        }

    }
}