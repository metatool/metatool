using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gma.System.MouseKeyHook;

namespace Metaseed.Input
{
    public class Keyboard
    {
        public static IKeyEvents Hotkey(string keys)
        {
            return KeyboardHook.Shortcut(keys);
        }

        public static void Hook()
        {
            KeyboardHook.Run();
        }
    }
}
