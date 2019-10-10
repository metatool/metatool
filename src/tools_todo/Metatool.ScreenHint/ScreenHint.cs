using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input;
using Metatool.ScreenPoint;
using Point = System.Drawing.Point;
using static Metatool.Input.Key;

namespace Metatool.ScreenHint
{
    public sealed class ScreenHint
    {
        private readonly IKeyboard _keyboard;
        private readonly IMouse _mouse;

        public ScreenHint(IKeyboard keyboard, IMouse mouse)
        {
            _keyboard = keyboard;
            _mouse = mouse;

            MouseClick = (Ctrl + Alt + X).Down(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => Show(MouseLeftClick));
            });

            MouseClickLast = (Ctrl + Alt + Z).Down(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => Show(MouseLeftClick, false));
            });

        }
        static (Rect windowRect, Dictionary<string, Rect> rects) _positions;

        public  async void Show(Action<(Rect winRect, Rect clientRect)> action, bool buildHints = true)
        {
            buildHints = buildHints || _positions.Equals(default);
            if (buildHints)
            {
                var builder = new HintsBuilder();
                _positions = builder.BuildHintPositions();
                HintUI.Inst.CreateHint(_positions);
                HintUI.Inst.Show();
            }
            else
            {
                HintUI.Inst.Show(true);
            }


            var hits = new StringBuilder();
            while (true)
            {
                var downArg = await _keyboard.KeyDownAsync(true);

                if (downArg.KeyCode == Keys.LShiftKey)
                {
                    HintUI.Inst.HideHints();
                    var upArg = await _keyboard.KeyUpAsync();
                    HintUI.Inst.ShowHints();
                    continue;
                }

                var downKey = downArg.KeyCode.ToString();
                if (downKey.Length > 1 || !Config.Keys.Contains(downKey))
                {
                    HintUI.Inst.Hide();
                    return;
                }

                hits.Append(downKey);
                var ks = new List<string>();
                foreach (var k in _positions.rects.Keys)
                {
                    if (k.StartsWith(hits.ToString()))
                    {
                        HintUI.Inst.MarkHit(k, hits.Length);
                        ks.Add(k);
                    }
                    else
                    {
                        HintUI.Inst.HideHint(k);
                    }
                }

                if (ks.Count == 0)
                {
                    HintUI.Inst.Hide();
                    return;
                }

                var key = ks.FirstOrDefault(k => k.Length == hits.Length);
                if (!string.IsNullOrEmpty(key))
                {
                    var v = _positions.rects[key];
                    HintUI.Inst.HideHints();
                    HintUI.Inst.HighLight(v);

                    await Task.Run(()=>
                    {
                        Thread.Sleep(100);
                        action((_positions.windowRect, v));
                    });
                    return;
                }
            }
        }

        void MouseLeftClick((Rect winRect, Rect clientRect) position)
        {
            var rect    = position.clientRect;
            var winRect = position.winRect;
            rect.X = winRect.X + rect.X;
            rect.Y = winRect.Y + rect.Y;
            var p = new Point((int) (rect.X + rect.Width / 2), (int) (rect.Y + rect.Height / 2));
            _mouse.Position = p;
            _mouse.LeftClick();
        }

        public IKeyboardCommandToken  MouseClick;
        public IKeyboardCommandToken MouseClickLast;
    }
}