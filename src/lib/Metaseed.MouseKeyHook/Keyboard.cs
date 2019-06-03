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
