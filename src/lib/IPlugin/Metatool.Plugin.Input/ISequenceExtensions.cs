using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Metatool.Command;
using Metatool.Plugin;

namespace Metatool.Input
{
    public static class ISequenceExtensions
    {
        private static IKeyboard _default;

        private static IKeyboard Default =>
            _default ??= (ServiceLocator.Current.GetService(typeof(IKeyboard)) as IKeyboard);

        private static ICommandManager _commandManager;

        private static ICommandManager CommandManager =>
            _commandManager ??= (ServiceLocator.Current.GetService(typeof(ICommandManager)) as ICommandManager);

        public static IKeyboardCommandToken  Down(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Down(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Down(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Down(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Down(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Down(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Down(this ISequence sequence,
            Action<IKeyEventArgs> execute, Predicate<IKeyEventArgs> canExecute=null, string description = "",
            KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            Debug.Assert(sequence != null, nameof(sequence) + " != null");
            var trigger = Default.Down(sequence); 
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Default;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static IKeyboardCommandToken  Up(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Up(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Up(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Up(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Up(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Up(action, canExecute, description);
        }

        public static IKeyboardCommandToken  Up(this ISequence sequence, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute=null,
            string description = "", KeyStateTrees stateTree = KeyStateTrees.Default)
        {
            Debug.Assert(sequence != null, nameof(sequence) + " != null");
            var trigger = Default.Up(sequence);
            var token   = CommandManager.Add(trigger, execute, canExecute, description); 
            var keyboardInternal = (IKeyboardInternal)Default;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static Task<IKeyEventArgs> DownAsync(this ISequence sequence, int timeout = 8888)
        {
            var last = sequence.Last();
            sequence.Down(null);
            return last.DownAsync(timeout);
        }

        public static Task<IKeyEventArgs> UpAsync(this ISequence sequence, int timeout = 8888)
        {
            var last = sequence.Last();
            sequence.Up(null);
            return last.UpAsync(timeout);
        }
    }
}
