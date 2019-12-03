using System;
using Metatool.Command;

namespace Metatool.Service
{
    public static class IHotkeyValutTupleExtensions
    {
        public static IKeyCommand  Down(this ValueTuple<IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).OnDown(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(this ValueTuple<IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).OnDown(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(
            this ValueTuple<IHotkey, IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .OnDown(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Down(
            this ValueTuple<IHotkey, IHotkey, IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .OnDown(action, canExecute, description, stateTree);
        }

        public static IKeyCommand Down(
            this ValueTuple<IHotkey, IHotkey, IHotkey, IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Then(sequence.Item6)
                .OnDown(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).OnUp(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).OnUp(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(this ValueTuple<IHotkey, IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4)
                .OnUp(action, canExecute, description, stateTree);
        }

        public static IKeyCommand  Up(
            this ValueTuple<IHotkey, IHotkey, IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute=null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5)
                .OnUp(action, canExecute, description, stateTree);
        }

        public static IKeyCommand Up(
            this ValueTuple<IHotkey, IHotkey, IHotkey, IHotkey, IHotkey, IHotkey> sequence,
            Action<IKeyEventArgs> action, Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            return sequence.Item1.Then(sequence.Item2).Then(sequence.Item3).Then(sequence.Item4).Then(sequence.Item5).Then(sequence.Item6)
                .OnUp(action, canExecute, description, stateTree);
        }

    }
}
