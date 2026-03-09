using Metatool.Service;
using Metatool.Tools.KeyMouse;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using Point = System.Drawing.Point;
using static Metatool.Core.F;
using System.Windows.Input;
using Metatool.Service.ScreenHint;
using Microsoft.Extensions.DependencyInjection;

namespace Metatool.MetaKeyboard;

public class KeyboardMouseToolPackage : CommandPackage
{
    private readonly IMouse mouse;
    private readonly IWindowManager windowManager;
    public KeyboardMouseToolPackage(IServiceCollection services, IScreenHint screenHint, IMouse mouse, IWindowManager windowManager, IKeyboard keyboard, IConfig<KeyMousePluginConfig> config)
    {
        var conf = config.CurrentValue;
        var screenHintConfig = conf.KeyboardMousePackage.ScreenHintConfig;
        this.windowManager = windowManager;
        this.mouse = mouse;
        RegisterCommands();

        var maps = conf.KeyboardMousePackage.KeyMaps;
        keyboard.RegisterKeyMaps(maps);

        var hotkeys = conf.KeyboardMousePackage.Hotkeys;

        hotkeys.MouseToFocus.OnEvent(_ => MoveCursorToActiveControl());

        var moveCursorToActiveWindow = Run<Action>(() =>
        {
            var lastMouseOverWin_ = IntPtr.Zero;
            var lastCallTime_ = DateTime.MinValue;

            return () =>
            {
                var lastTime = lastCallTime_;
                lastCallTime_ = DateTime.Now;
                // filter out frequently call
                if (lastCallTime_ - lastTime < TimeSpan.FromSeconds(1))
                    return;

                var lastLastActiveWin = lastMouseOverWin_;
                var activeWin = windowManager.CurrentWindow;
                lastMouseOverWin_ = activeWin.Handle;
                if (lastLastActiveWin == lastMouseOverWin_)
                {
                    // user  may move the mouse away manually, so check again
                    var winWithCursor = windowManager.WindowWithMouse;
                    if (winWithCursor.Handle == activeWin.Handle)
                        return;
                }

                var r = activeWin.Rect;
                var x = (int)(r.X + r.Width / 2);
                var y = (int)(r.Y + r.Height / 2);
                if (x != 0 && y != 0)
                    //Task.Run(() => mouse.MoveToLikeUser(x, y));
                    mouse.Position = new Point(x, y);
            };
        });

        hotkeys.MouseScrollUp.OnEvent(e =>
        {
            moveCursorToActiveWindow();
            mouse.VerticalScroll(1);
        });

        hotkeys.MouseScrollDown.OnEvent(e =>
        {
            moveCursorToActiveWindow();
            mouse.VerticalScroll(-1);
        });

        var acceleratingChange = Run<Func<int, bool, int>>(() =>
        {
            var lastTime = DateTime.MinValue;
            var timeThreshold = TimeSpan.FromMilliseconds(100);
            var delta = conf.KeyboardMousePackage.MouseMoveDelta;
            var halfDelta = delta / 2;
            var maxDelta = delta * 5;

            return (value, increase) =>
            {
                var lastLastTime = lastTime;
                lastTime = DateTime.Now;
                if (lastTime - lastLastTime < timeThreshold)
                {
                    delta = Math.Min(delta + halfDelta, maxDelta);
                }
                else
                {
                    delta = conf.KeyboardMousePackage.MouseMoveDelta;
                }

                if (increase)
                    value += delta;
                else
                    value -= delta;

                return value;
            };
        });

        hotkeys.MouseLeft.OnEvent(e =>
        {
            mouse.Position = new Point(acceleratingChange(mouse.Position.X, false), mouse.Position.Y);
        });


        hotkeys.MouseRight.OnEvent(e =>
        {
            mouse.Position = new Point(acceleratingChange(mouse.Position.X, true), mouse.Position.Y);
        });

        hotkeys.MouseUp.OnEvent(e =>
        {
            mouse.Position = new Point(mouse.Position.X, acceleratingChange(mouse.Position.Y, false));
        });

        hotkeys.MouseDown.OnEvent(e =>
        {
            mouse.Position = new Point(mouse.Position.X, acceleratingChange(mouse.Position.Y, true));
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


    public void OnUnloading()
    {
    }

}