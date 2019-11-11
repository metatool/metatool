using Metatool.Input;
using Metatool.Plugin;
using static Metatool.Input.Key;

namespace Metatool.MetaKeyboard
{
    public class KeyboardConfig : CommandPackage
    {
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
        public static Key AK    = new Key(Key.Apps, Key.Tab);
    }
}
