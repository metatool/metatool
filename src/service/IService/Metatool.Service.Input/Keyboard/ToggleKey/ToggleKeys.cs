using Metatool.Service.Internal;

namespace Metatool.Service
{
    public class ToggleKeys
    {
        private static IKeyboardInternal _keyboard;

        private static IKeyboardInternal Keyboard =>
            _keyboard ??= Services.Get<IKeyboard, IKeyboardInternal>();

        public static IToggleKey NumLock    = Keyboard.GeToggleKey(Key.Num);
        public static IToggleKey CapsLock   = Keyboard.GeToggleKey(Key.Caps);
        public static IToggleKey ScrollLock = Keyboard.GeToggleKey(Key.Scroll);
        public static IToggleKey Insert     = Keyboard.GeToggleKey(Key.Ins);
    }
}
