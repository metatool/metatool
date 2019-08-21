using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.Input;
using static Metaseed.Input.Key;
namespace Metaseed.ScreenPoint
{
    internal class HotKey
    {
        public HotKey()
        {
            (Ctrl + Q).Down(e =>
            {
new PointBuilder().Run();
            });
            Keyboard.Hook();
        }
    }
}
