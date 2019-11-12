using System;
using System.Threading.Tasks;
using Metatool.Command;

namespace Metatool.Service
{
    public static class IHotKeyExtension
    {
        private static IKeyboard _keyboard;
        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();

        private static ICommandManager _commandManager;
        private static ICommandManager CommandManager =>
            _commandManager ??= Services.Get<ICommandManager>();

        public static IKeyCommand Down(this IHotkey hotkey, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var trigger = Keyboard.Down(hotkey, stateTree);
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }

        public static IKeyCommand Up(this IHotkey hotkey, Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var trigger = Keyboard.Up(hotkey, stateTree);
            var token = CommandManager.Add(trigger, execute, canExecute, description);
            var keyboardInternal = (IKeyboardInternal)Keyboard;
            return keyboardInternal.GetToken(token, trigger);
        }
        /// <summary>
        /// register the key to the state tree, and wait the down event;
        /// timeout: return null
        /// </summary>
        public static Task<IKeyEventArgs> DownAsync(this IHotkey hotkey, int timeout = -1, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var command = new KeyEventAsync();
            hotkey.Down(command.OnEvent, null, description, stateTree);
            return command.WaitAsync(timeout);
        }

        public static Task<IKeyEventArgs> UpAsync(this IHotkey sequenceUnit, int timeout = -1, string description = "", string stateTree = KeyStateTrees.Default)
        {
            var command = new KeyEventAsync();
            sequenceUnit.Up(command.OnEvent, null, description, stateTree);
            return command.WaitAsync(timeout);
        }

    }
}
