﻿using System;
using System.Threading.Tasks;
using Metatool.Command;
using Metatool.Service;

namespace Metatool.Input
{
    public static class ISequenceUnitExtetions
    {
        private static IKeyboard _keyboard;
        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();

        private static ICommandManager _commandManager;
        private static ICommandManager CommandManager =>
            _commandManager ??= Services.Get<ICommandManager>();

        public static IKeyCommand Down(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger = Keyboard.Down(combination, stateTree);
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal) Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static IKeyCommand  Up(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree= KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger = Keyboard.Up(combination, stateTree);
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }
        /// <summary>
        /// register the key to the state tree, and wait the down event;
        /// timeout: return null
        /// </summary>
        public static Task<IKeyEventArgs> DownAsync(this ISequenceUnit sequenceUnit, int timeout = -1, string description  ="", string stateTree = KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var command = new KeyEventAsync();
            var trigger = Keyboard.Down(combination, stateTree);
            CommandManager.Add(trigger, command.OnEvent, null, description);
            return command.WaitAsync(timeout);
        }

        public static Task<IKeyEventArgs> UpAsync(this ISequenceUnit sequenceUnit, int timeout = -1, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var command     = new KeyEventAsync();
            var trigger     = Keyboard.Up(combination, stateTree);
            CommandManager.Add(trigger, command.OnEvent, null, description);
            return command.WaitAsync(timeout);
        }

        public static IKeyCommand  AllUp(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            if (sequenceUnit is Key) throw new Exception("AllUp event could only be used on Key, please use Up event!");
            var combination = sequenceUnit.ToCombination();
            var trigger     = Keyboard.AllUp(combination, stateTree);
            var token       = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static IKeyCommand  Hit(this ISequenceUnit sequenceUnit, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute, string description, string stateTree = KeyStateTrees.Default)
        {
            var combination = sequenceUnit.ToCombination();
            var trigger     = Keyboard.Hit(combination,stateTree);
            var token       = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }


        public static IKeyCommand  Map(this ISequenceUnit key, Key target, Predicate<IKeyEventArgs> canExecute = null,
            int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), new Combination(target), canExecute, repeat);
        }

        public static IKeyCommand  Map(this ISequenceUnit key, ICombination target,
            Predicate<IKeyEventArgs> canExecute = null, int repeat = 1)
        {
            return Keyboard.Map(key.ToCombination(), target, canExecute, repeat);
        }
        public static IKeyCommand HardMap(this ISequenceUnit key, Key target, Predicate<IKeyEventArgs> canExecute = null)
        {
            return Keyboard.HardMap(key.ToCombination(), new Combination(target), canExecute);
        }

        public static IKeyCommand  MapOnHit(this ISequenceUnit key, Key target,
            Predicate<IKeyEventArgs> canExecute = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), new Combination(target), canExecute, allUp);
        }

        public static IKeyCommand  MapOnHit(this ISequenceUnit key, ICombination target,
            Predicate<IKeyEventArgs> canExecute = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(key.ToCombination(), target, canExecute, allUp);
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
