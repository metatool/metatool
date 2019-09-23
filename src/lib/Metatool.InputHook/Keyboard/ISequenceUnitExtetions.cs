﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Metatool.Input.MouseKeyHook.Implementation;

namespace Metatool.Input
{
    public static class ISequenceUnitExtetions
    {
        public static IMetaKey Down(this ISequenceUnit sequenceUnit, Action<KeyEventArgsExt> execute,
            Predicate<KeyEventArgsExt> canExecute = null, string description = "", KeyStateTree stateTree = null)
        {
            var combination = sequenceUnit.ToCombination();
            return Keyboard.Add(combination, KeyEvent.Down,
                new KeyCommand(execute) {CanExecute = canExecute, Description = description}, stateTree);
        }

        public static IMetaKey Up(this ISequenceUnit sequenceUnit, Action<KeyEventArgsExt> execute,
            Predicate<KeyEventArgsExt> canExecute = null, string description = "", KeyStateTree stateTree = null)
        {
            var combination = sequenceUnit.ToCombination();

            return Keyboard.Add(combination, KeyEvent.Up,
                new KeyCommand(execute) {CanExecute = canExecute, Description = description}, stateTree);
        }

        public static IMetaKey AllUp(this ISequenceUnit sequenceUnit, Action<KeyEventArgsExt> execute,
            Predicate<KeyEventArgsExt> canExecute = null, string description = "", KeyStateTree stateTree = null)
        {
            if (sequenceUnit is Key) throw new Exception("AllUp event could only be used on Key, please use Up event!");
            var combination = sequenceUnit.ToCombination();
            return Keyboard.Add(combination, KeyEvent.AllUp,
                new KeyCommand(execute) {CanExecute = canExecute, Description = description}, stateTree);
        }

        public static IMetaKey Hit(this ISequenceUnit sequenceUnit, Action<KeyEventArgsExt> execute,
            Predicate<KeyEventArgsExt> canExecute, string description, bool markHandled = true)
        {
            var combination = sequenceUnit.ToCombination();

            var keyAction = new KeyCommand(execute) {Description = description};
            return Keyboard.Hit(combination, keyAction, canExecute, markHandled);
        }

        public static IMetaKey Map(this ISequenceUnit key, Keys target, Predicate<KeyEventArgsExt> canExecute = null,
            int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), new Combination(target), canExecute, repeat);
        }

        public static IMetaKey Map(this ISequenceUnit key, Key target, Predicate<KeyEventArgsExt> canExecute = null,
            int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), new Combination(target), canExecute, repeat);
        }

        public static IMetaKey HardMap(this ISequenceUnit key, Key target, Predicate<KeyEventArgsExt> canExecute = null)
        {
            return Keyboard.HardMap(key.ToCombination(), new Combination(target), canExecute);
        }

        public static IMetaKey Map(this ISequenceUnit key, ICombination target,
            Predicate<KeyEventArgsExt> canExecute = null, int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), target, canExecute, repeat);
        }

        public static IMetaKey MapOnHit(this ISequenceUnit key, Keys target,
            Predicate<KeyEventArgsExt> canExecute = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), new Combination(target), canExecute, allUp);
        }

        public static IMetaKey MapOnHit(this ISequenceUnit key, ICombination target,
            Predicate<KeyEventArgsExt> canExecute = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), target, canExecute, allUp);
        }

        public static ICombination Handled(this ISequenceUnit sequenceUnit, KeyEvent keyEvent = KeyEvent.Down | KeyEvent.Up | KeyEvent.AllUp)
        {
            if((keyEvent & KeyEvent.Down) == KeyEvent.Down)
                sequenceUnit.Down(e => e.Handled = true);
            if ((keyEvent & KeyEvent.Up) == KeyEvent.Up)
                sequenceUnit.Up(e => e.Handled   = true);
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
        public static Task<KeyEventArgsExt> DownAsync(this ISequenceUnit sequenceUnit, int timeout = -1)
        {
            var comb    =sequenceUnit.ToCombination();
            var command = new KeyEventAsync(KeyEvent.Down);
            Keyboard.Add(comb, KeyEvent.Down, new KeyCommand(command.OnEvent));
            return command.WaitAsync(timeout);
        }

        public static Task<KeyEventArgsExt> UpAsync(this ISequenceUnit sequenceUnit, int timeout = -1)
        {
            var comb = sequenceUnit.ToCombination();
            var command = new KeyEventAsync(KeyEvent.Up);
            Keyboard.Add(comb, KeyEvent.Up, new KeyCommand(command.OnEvent));
            return command.WaitAsync(timeout);
        }
    }
}