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
            Apps.AsChordKey();
            Space.AsChordKey();
            Enter.AsChordKey();
            Tab.AsChordKey();
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
