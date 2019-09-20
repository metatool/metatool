using Metatool.Input;
using static Metatool.Input.Key;

namespace Metatool.MetaKeyboard
{
    public class KeyboardConfig : KeyMetaPackage
    {
        // todo: auto convert to Chord, when used in Chord.
        public IMetaKey AppsAsChord  = Apps.AsChordKey();
        public IMetaKey SpaceAsChord = Space.AsChordKey();
        public IMetaKey EnterAsChord = Enter.AsChordKey();
        public IMetaKey TabAsChord   = Tab.AsChordKey();
        public IMetaKey PipeAsChord = Pipe.AsChordKey();

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
