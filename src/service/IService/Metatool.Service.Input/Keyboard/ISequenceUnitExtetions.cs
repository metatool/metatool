using System;
using Metatool.Command;

namespace Metatool.Service
{
    public static class ISequenceUnitExtetions
    {
        private static IKeyboard _keyboard;
        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();

        private static ICommandManager _commandManager;
        private static ICommandManager CommandManager =>
            _commandManager ??= Services.Get<ICommandManager>();



        public static IKeyCommand HardMap(this ISequenceUnit key, Key target, Predicate<IKeyEventArgs> canExecute = null)
        {
            return Keyboard.HardMap(key.ToCombination(), new Combination(target), canExecute);
        }
      
    }
}
