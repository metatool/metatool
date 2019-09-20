using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Metatool.Input;
using Metatool.ScreenPoint;
using Keyboard = Metatool.Input.Keyboard;
using Point = System.Drawing.Point;
using static Metatool.Input.Key;
using Application = System.Windows.Application;

namespace Metatool.ScreenHint
{
    public class ScreenHint
    {
        static (Rect windowRect, Dictionary<string, Rect> rects) _positions;

        public static async void Show(Action<(Rect winRect, Rect clientRect)> action, bool buildHints = true)
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
                var downArg = await Keyboard.KeyDownAsync(true);

                if (downArg.KeyCode == Keys.LShiftKey)
                {
                    HintUI.Inst.HideHints();
                    var upArg = await Keyboard.KeyUpAsync();
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

        static void MouseLeftClick((Rect winRect, Rect clientRect) position)
        {
            var rect    = position.clientRect;
            var winRect = position.winRect;
            rect.X = winRect.X + rect.X;
            rect.Y = winRect.Y + rect.Y;
            var p = new Point((int) (rect.X + rect.Width / 2), (int) (rect.Y + rect.Height / 2));
            Input.Mouse.Simu.Position = p;
            Input.Mouse.Simu.LeftClick();
        }

        public IMetaKey MouseClick = (Ctrl + Alt + X).Down(e =>
        {
            e.Handled = true;
            e.BeginInvoke(() => ScreenHint.Show(MouseLeftClick));
        });

        public IMetaKey MouseClickLast = (Ctrl + Alt + Z).Down(e =>
        {
            e.Handled = true;
            e.BeginInvoke(() => ScreenHint.Show(MouseLeftClick, false));
        });

        public void Hook()
        {
            Keyboard.Hook();
        }
    }
}