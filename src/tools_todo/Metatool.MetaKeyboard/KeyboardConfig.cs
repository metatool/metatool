using Metatool.Command;
using Metatool.Input;
using static Metatool.Input.Key;

namespace Metatool.MetaKeyboard
{
    public class KeyboardConfig : KeyMetaPackage
    {
        // todo: auto convert to Chord, when used in Chord.
        public IKeyboardCommandToken  AppsAsChord  = Apps.AsChordKey();
        public IKeyboardCommandToken  SpaceAsChord = Space.AsChordKey();
        public IKeyboardCommandToken  EnterAsChord = Enter.AsChordKey();
        public IKeyboardCommandToken  TabAsChord   = Tab.AsChordKey();
        public IKeyboardCommandToken  PipeAsChord = Pipe.AsChordKey();

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
