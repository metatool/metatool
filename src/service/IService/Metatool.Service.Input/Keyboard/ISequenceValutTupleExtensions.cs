using System;
using Metatool.Command;

namespace Metatool.Service
{
    public static class ISequenceValutTupleExtensions
    {
        public static IKeyCommand  Down(this ValueTuple<IHotkey, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(
            this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(
            this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand Down(
            this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Then(sequence.Item6)
                .Down(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<IHotkey, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(
            this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .Up(action, canExecute, description, stateTree);
        }

        public static IKeyCommand Up(
            this ValueTuple<IHotkey, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit, ISequenceUnit> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Then(sequence.Item6)
                .Up(action, canExecute, description, stateTree);
        }

    }
}
