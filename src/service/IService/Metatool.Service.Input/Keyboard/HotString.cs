using System;
using Metatool.Command;

namespace Metatool.Service
{
    public static class HotString
    {
        private static IKeyboard _keyboard;
        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();
        public static IKeyCommand Map(this string source, string target, Predicate<IKeyEventArgs> predicate = null)
        {
            return Keyboard.Map(source, target, e=> !e.IsVirtual);
        }
    }
}
