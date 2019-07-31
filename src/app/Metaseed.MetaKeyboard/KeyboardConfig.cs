using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.Input;
using static Metaseed.Input.Key;
namespace Metaseed.MetaKeyboard
{
    public class KeyboardConfig
    {
        static KeyboardConfig()
        {
            Apps.ToChoreKey();
            Space.ToChoreKey();
        }

        public static Key GK = new Key(Space);
        public static Key CK = new Key(Caps, Apps);
    }
}
