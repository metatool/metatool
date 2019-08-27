using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using FlaUI.Core.Input;
using Metaseed.Input;
using Metaseed.ScreenPoint;
using Keyboard = Metaseed.Input.Keyboard;
using Mouse = FlaUI.Core.Input.Mouse;
using Point = System.Drawing.Point;

namespace Metaseed.ScreenHint
{
    public class Hint
    {
        static (Rect windowRect, Dictionary<string, Rect> rects) _positions;

        public static async void Show(bool buildHints = true)
        {
            buildHints = buildHints || _positions.Equals(default);
            if (buildHints)
            {
                var builder = new HintsBuilder();
                _positions = builder.BuildHintPositions();
                HintUI.Inst.CreateHint(_positions);
            }

            HintUI.Inst.Show();

            var hits = new StringBuilder();
            while (true)
            {
                var downArg = await Keyboard.KeyDownAsync();
                downArg.Handled = true;

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
                    HintUI.Inst.HighLight(v);
                    v.X = _positions.windowRect.X + v.X;
                    v.Y = _positions.windowRect.Y + v.Y;
                    var p = new Point((int) (v.X + v.Width / 2), (int) (v.Y + v.Height / 2));
                    Mouse.Position = p;
                    Wait.UntilInputIsProcessed();
                    HintUI.Inst.HideHints();
                    Mouse.LeftClick();
                    return;
                }
            }
        }

        public void Hook()
        {
            (Key.Ctrl + Key.S).Down(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => Show());
            });

            (Key.Ctrl + Key.Q).Down(e =>
            {
                e.Handled = true;
                e.BeginInvoke(() => Show(false));
            });

            Keyboard.Hook();
        }
    }
}