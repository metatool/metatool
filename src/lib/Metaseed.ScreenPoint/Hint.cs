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
                _positions = builder.BuildHintPositions(ScreenHint.MainWindow.Inst);
                builder.CreateHint(ScreenHint.MainWindow.Inst, _positions);
            }

            ScreenHint.MainWindow.Inst.Show();

            var str = new StringBuilder();
            while (true)
            {
                var downArg = await Keyboard.KeyDownAsync();
                downArg.Handled = true;

                if (downArg.KeyCode == Keys.LShiftKey)
                {
                    ScreenHint.MainWindow.Inst.Canvas.Visibility = Visibility.Hidden;
                    var upArg = await Keyboard.KeyUpAsync();
                    ScreenHint.MainWindow.Inst.Canvas.Visibility = Visibility.Visible;
                    continue;
                }

                var downKey = downArg.KeyCode.ToString();
                if (downKey.Length > 1 || !Config.Keys.Contains(downKey))
                {
                    ScreenHint.MainWindow.Inst.Hide();
                    return;
                }

                str.Append(downKey);
                var ks = _positions.rects.Keys.Where(k => k.StartsWith(str.ToString())).ToArray();
                if (ks.Length == 0)
                {
                    ScreenHint.MainWindow.Inst.Hide();
                    return;
                }

                var key = ks.FirstOrDefault(k => k.Length == str.Length);
                if (!string.IsNullOrEmpty(key))
                {
                    var v = _positions.rects[key];
                    v.X = _positions.windowRect.X + v.X;
                    v.Y = _positions.windowRect.Y + v.Y;
                    var p = new Point((int)(v.X + v.Width / 2), (int)(v.Y + v.Height / 2));
                    Mouse.Position = p;
                    Wait.UntilInputIsProcessed();
                    ScreenHint.MainWindow.Inst.Hide();
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