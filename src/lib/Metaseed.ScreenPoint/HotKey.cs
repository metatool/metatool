using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlaUI.Core.Tools;
using Metaseed.Input;
using static Metaseed.Input.Key;
using Mouse = FlaUI.Core.Input.Mouse;

namespace Metaseed.ScreenPoint
{
    internal class HotKey
    {
        public HotKey()
        {
            (Ctrl + Q).Down(async e =>
            {
                var             r = new PointBuilder().Run();
                KeyEventArgsExt a;
                var   str = new StringBuilder();

                e.BeginInvoke(async () =>
                {
                    while (true)
                    {
                        a = await Keyboard.KeyDownAsync();

                        var k = a.KeyCode.ToString();
                        if (k.Length > 1 || !Config.Keys.Contains(k))
                        {
                            r.window.Close();
                            return;
                        }

                        str.Append(k);
                        var ks = r.dic.Keys.Where(k => k.StartsWith(str.ToString())).ToArray();
                        if (ks.Length == 0)
                        {
                            r.window.Close();
                            return;
                        }

                        var key = ks.FirstOrDefault(k => k.Length == str.Length);
                        if (!string.IsNullOrEmpty(key))
                        {
                            var v = r.dic[key];
                            var p = v.element.BoundingRectangle.Center();
                            Mouse.MoveTo(p);
                            Mouse.LeftClick();
                            r.window.Close();
                            return;
                        }

                    }
                });
            });
            Keyboard.Hook();
        }
    }
}