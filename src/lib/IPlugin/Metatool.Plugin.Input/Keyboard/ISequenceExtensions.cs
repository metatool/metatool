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
            _default ??= Services.Get<IKeyboard>();

        private static ICommandManager _commandManager;
        private static ICommandManager CommandManager =>
            _commandManager ??= Services.Get<ICommandManager>();

        public static IKeyCommand  Down(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Then(sequence.Item6)
                .Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand Up(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Then(sequence.Item6)
                .Up(action, canExecute, description, stateTree);
        }




        public static IKeyCommand Down(this ISequence sequence,
            Action<IKeyEventArgs> execute, Predicate<IKeyEventArgs> canExecute = null, string description = "",
            string stateTree = KeyStateTrees.Default)
        {
            Debug.Assert(sequence != null, nameof(sequence) + " != null");
            var trigger          = Default.Down(sequence, stateTree);
            var token            = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Default;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static IKeyCommand  Up(this ISequence sequence, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute=null,
            string description = "", string stateTree = KeyStateTrees.Default)
        {
            Debug.Assert(sequence != null, nameof(sequence) + " != null");
            var trigger = Default.Up(sequence, stateTree);
            var token   = CommandManager.Add(trigger, execute, canExecute, description); 
            var keyboardInternal = (IKeyboardInternal)Default;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static Task<IKeyEventArgs> DownAsync(this ISequence sequence, int timeout = 8888, string description="", string stateTree = KeyStateTrees.Default)
        {
            var command = new KeyEventAsync();
            var trigger = sequence.Down(command.OnEvent,null,description, stateTree);
            return command.WaitAsync(timeout);
        }

        public static Task<IKeyEventArgs> UpAsync(this ISequence sequence, int timeout = 8888, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var command = new KeyEventAsync();
            var trigger = sequence.Up(command.OnEvent, null, description, stateTree);
            return command.WaitAsync(timeout);
        }
    }
}
