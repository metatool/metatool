using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metatool.Command;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Plugin;

namespace Metatool.Input
{
    public static class ISequenceUnitExtetions
    {
        private static Keyboard _default;

        private static Keyboard Default =>
            _default ??= (ServiceLocator.Current.GetService(typeof(IKeyboard)) as Keyboard);

        private static ICommandManager _commandManager;

        private static ICommandManager CommandManager =>
            _commandManager ??= (ServiceLocator.Current.GetService(typeof(ICommandManager)) as ICommandManager);

        public static IKeyboardCommandToken Down(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", KeyStateTree stateTree = null)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger = Default.Down(combination);
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            return new KeyboardCommandToken(token, trigger);
        }

        public static IKeyboardCommandToken  Up(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", KeyStateTree stateTree = null)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger = Default.Up(combination);
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            return new KeyboardCommandToken(token, trigger);
        }

        public static IKeyboardCommandToken  AllUp(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", KeyStateTree stateTree = null)
        {
            if (sequenceUnit is Key) throw new Exception("AllUp event could only be used on Key, please use Up event!");
            var combination = sequenceUnit.ToCombination();
            var trigger     = Default.AllUp(combination);
            var token       = CommandManager.Add(trigger, execute, canExecute, description);
            return new KeyboardCommandToken(token, trigger);
        }

        public static IKeyboardCommandToken  Hit(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute, string description, bool markHandled = true)
        {
            var combination = sequenceUnit.ToCombination();

            var keyAction = new KeyCommand(execute) {Description = description};
            return Default.Hit(combination, keyAction, canExecute, markHandled);
        }

        public static IKeyboardCommandToken  Map(this ISequenceUnit key, Keys target, Predicate<IKeyEventArgs> canExecute = null,
            int repeat = 1)
        {
            return Default.Map(key.ToCombination(), new Combination(target), canExecute, repeat);
        }

        public static IKeyboardCommandToken  Map(this ISequenceUnit key, Key target, Predicate<IKeyEventArgs> canExecute = null,
            int repeat = 1)
        {
            return Default.Map(key.ToCombination(), new Combination(target), canExecute, repeat);
        }

        public static IKeyboardCommandToken  HardMap(this ISequenceUnit key, Key target, Predicate<IKeyEventArgs> canExecute = null)
        {
            return Default.HardMap(key.ToCombination(), new Combination(target), canExecute);
        }

        public static IKeyboardCommandToken  Map(this ISequenceUnit key, ICombination target,
            Predicate<IKeyEventArgs> canExecute = null, int repeat = 1)
        {
            return Default.Map(key.ToCombination(), target, canExecute, repeat);
        }

        public static IKeyboardCommandToken  MapOnHit(this ISequenceUnit key, Keys target,
            Predicate<IKeyEventArgs> canExecute = null, bool allUp = true)
        {
            return Default.MapOnHit(key.ToCombination(), new Combination(target), canExecute, allUp);
        }

        public static IKeyboardCommandToken  MapOnHit(this ISequenceUnit key, ICombination target,
            Predicate<IKeyEventArgs> canExecute = null, bool allUp = true)
        {
            return Default.MapOnHit(key.ToCombination(), target, canExecute, allUp);
        }

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

        /// <summary>
        /// register the key to the state tree, and wait the down event;
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Task<IKeyEventArgs> DownAsync(this ISequenceUnit sequenceUnit, int timeout = -1)
        {
            var comb    = sequenceUnit.ToCombination();
            var command = new KeyEventAsync(KeyEvent.Down);
            Default.Add(comb, KeyEvent.Down, new KeyCommand(command.OnEvent));
            return command.WaitAsync(timeout);
        }

        public static Task<IKeyEventArgs> UpAsync(this ISequenceUnit sequenceUnit, int timeout = -1)
        {
            var comb    = sequenceUnit.ToCombination();
            var command = new KeyEventAsync(KeyEvent.Up);
            Default.Add(comb, KeyEvent.Up, new KeyCommand(command.OnEvent));
            return command.WaitAsync(timeout);
        }
    }
}