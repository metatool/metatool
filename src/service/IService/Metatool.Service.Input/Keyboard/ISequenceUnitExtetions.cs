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

        public static IKeyCommand  AllUp(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            if (sequenceUnit is Key) throw new Exception("AllUp event could only be used on Key, please use Up event!");
            var trigger     = Keyboard.AllUp(sequenceUnit, stateTree);
            var token       = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static IKeyCommand  Hit(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute, string description, string stateTree = KeyStateTrees.Default)
        {
            var trigger     = Keyboard.Hit(sequenceUnit, stateTree);
            var token       = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }


        public static IKeyCommand  Map(this ISequenceUnit key, ISequenceUnit target, Predicate<IKeyEventArgs> canExecute = null,
            int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), target.ToCombination(), canExecute, repeat);
        }

        public static IKeyCommand HardMap(this ISequenceUnit key, Key target, Predicate<IKeyEventArgs> canExecute = null)
        {
            return Keyboard.HardMap(key.ToCombination(), new Combination(target), canExecute);
        }

        public static IKeyCommand  MapOnHit(this ISequenceUnit key, ISequenceUnit target,
            Predicate<IKeyEventArgs> canExecute = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), target.ToCombination(), canExecute, allUp);
        }

        // if the handler async run, this is needed.
        public static ICombination Handled(this ISequenceUnit sequenceUnit,
            KeyEvent keyEvent = KeyEvent.Down | KeyEvent.Up | KeyEvent.AllUp)
        {
            if ((keyEvent & KeyEvent.Down) == KeyEvent.Down)
                sequenceUnit.Down(e => e.Handled = true);
            if ((keyEvent & KeyEvent.Up) == KeyEvent.Up)
                sequenceUnit.Up(e => e.Handled = true);
            if ((keyEvent & KeyEvent.AllUp) == KeyEvent.AllUp)
                sequenceUnit.AllUp(e => e.Handled = true);
            return sequenceUnit.ToCombination();
        }

      
    }
}
