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
            Enter.ToChoreKey();
            Tab.ToChoreKey();
        }

        /// <summary>
        /// Global key
        /// </summary>
        public static Key GK = new Key(Space);
        /// <summary>
        /// Context key
        /// </summary>
        public static Key CK = new Key(Caps, Enter);
        /// <summary>
        /// Apps key
        /// </summary>
        public static Key AK = new Key(Key.Apps,Key.Tab);
    }
}
