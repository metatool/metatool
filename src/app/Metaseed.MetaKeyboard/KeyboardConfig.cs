using Metaseed.Input;
using static Metaseed.Input.Key;

namespace Metaseed.MetaKeyboard
{
    public class KeyboardConfig : KeyMetaPackage
    {
        public IMetaKey AppsAsChord  = Apps.AsChordKey();
        public IMetaKey SpaceAsChord = Space.AsChordKey();
        public IMetaKey EnterAsChord = Enter.AsChordKey();
        public IMetaKey TabAsChord   = Tab.AsChordKey();

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