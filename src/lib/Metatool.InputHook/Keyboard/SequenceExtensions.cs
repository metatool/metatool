using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.Input
{
    public static class SequenceExtensions
    {
        public static IMetaKey Down(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Down(action, canExecute, description);
        }

        public static IMetaKey Down(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Down(action, canExecute, description);
        }

        public static IMetaKey Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Down(action, canExecute, description);
        }

        public static IMetaKey Down(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Down(action, canExecute, description);
        }

        public static IMetaKey Down(this ISequence sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "",
            KeyStateTree stateTree = null)
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            return Keyboard.Default.Add(seq.ToList(), KeyEvent.Down,
                new KeyCommand(action) {CanExecute = canExecute, Description = description}, stateTree);
        }

        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Up(action, canExecute, description);
        }

        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Up(action, canExecute, description);
        }

        public static IMetaKey Up(this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Up(action, canExecute, description);
        }

        public static IMetaKey Up(
            this ValueTuple<ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "")
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Up(action, canExecute, description);
        }

        public static IMetaKey Up(this ISequence sequence, Action<IKeyEventArgs> action,
            Predicate<IKeyEventArgs> canExecute=null,
            string description = "", KeyStateTree stateTree = null)
        {
            var seq = sequence as Sequence;
            Debug.Assert(seq != null, nameof(sequence) + " != null");
            return Keyboard.Default.Add(seq.ToList(), KeyEvent.Up,
                new KeyCommand(action) {CanExecute = canExecute, Description = description}, stateTree);
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
