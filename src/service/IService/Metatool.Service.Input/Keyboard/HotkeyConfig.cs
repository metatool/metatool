using System;
using Metatool.Command;

namespace Metatool.Service
{
    public class HotkeyConfig
    {
        private static IKeyboard _keyboard;

        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();

        private static ICommandManager _commandManager;

        private static ICommandManager CommandManager =>
            _commandManager ??= Services.Get<ICommandManager>();

        private string    _hotKey;
        private ISequence HotKeyTrigger { get; set; }

        public string HotKey
        {
            get => _hotKey;
            set
            {
                _hotKey       = value;
                HotKeyTrigger = Sequence.Parse(value);
            }
        }

        public KeyEvent KeyEvent    { get; set; }
        public string   Description { get; set; }
        public string   StateTree   { get; set; } = KeyStateTrees.Default;
        public bool Handled { get; set; } = false;
        public bool     Enabled     { get; set; } = true;

        public IKeyCommand Register(Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null)
        {
            if (Handled) HotKeyTrigger.Handled(KeyEvent);
            var trigger = Keyboard.Event(HotKeyTrigger, KeyEvent, StateTree);
            var token   = trigger.Register(execute, canExecute, Description);
            return token;
        }
    }
}