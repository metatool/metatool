using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using Metaseed.Input;
using static Metaseed.Input.Key;
using Keyboard = Metaseed.Input.Keyboard;
using Mouse = FlaUI.Core.Input.Mouse;
using Point = System.Drawing.Point;

namespace Metaseed.ScreenPoint
{
    internal class HotKey
    {
        public HotKey()
        {
            (Ctrl + Q).Down(e =>
            {
                e.BeginInvoke(async () =>
                {
                    var r   = new PointBuilder().Run(MainWindow.Inst);
                    var str = new StringBuilder();
                    while (true)
                    {
                        var downArg = await Keyboard.KeyDownAsync();

                        if (downArg.KeyCode == Keys.LShiftKey)
                        {
                            MainWindow.Inst.Canvas.Visibility = Visibility.Hidden;
                            var upArg = await Keyboard.KeyUpAsync();
                            MainWindow.Inst.Canvas.Visibility = Visibility.Visible;
                            continue;
                        }

                        var downKey = downArg.KeyCode.ToString();
                        if (downKey.Length > 1 || !Config.Keys.Contains(downKey))
                        {
                            MainWindow.Inst.Hide();
                            return;
                        }

                        str.Append(downKey);
                        var ks = r.rects.Keys.Where(k => k.StartsWith(str.ToString())).ToArray();
                        if (ks.Length == 0)
                        {
                            MainWindow.Inst.Hide();
                            return;
                        }

                        var key = ks.FirstOrDefault(k => k.Length == str.Length);
                        if (!string.IsNullOrEmpty(key))
                        {
                            var v = r.rects[key];
                            v.X = r.windowRect.X + v.X;
                            v.Y = r.windowRect.Y + v.Y;
                            var p = new Point((int) (v.X + v.Width / 2), (int) (v.Y + v.Height / 2));
                            Mouse.Position = p;
                            Wait.UntilInputIsProcessed();
                            MainWindow.Inst.Hide();
                            Mouse.LeftClick();
                            return;
                        }
                    }
                });
            });
            Keyboard.Hook();
        }
    }
}