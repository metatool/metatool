﻿using System;
using System.Collections.Generic;
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

        private ISequence HotKeyTrigger
        {
            get
            {
                var hotkey = Keyboard.ReplaceAlias(HotKey, _tempAliasesDics);
                return Sequence.Parse(hotkey);
            }
        }

        public string HotKey { get; set; }

        public KeyEvent KeyEvent    { get; set; }
        public string   Description { get; set; }
        public string   StateTree   { get; set; } = KeyStateTrees.Default;
        public bool     Enabled     { get; set; } = true;

        public IKeyCommand Event(Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null)
        {
            var trigger = Keyboard.Event(HotKeyTrigger, KeyEvent, StateTree);
                                    var token   = trigger.Register(execute, canExecute, Description);
            return token;
        }

        public IKeyCommand MapOnHit(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null, bool allUp = true)
        {
            return Keyboard.MapOnHit(HotKeyTrigger, target, predicate, allUp);
        }

        private IDictionary<string, string>[] _tempAliasesDics;
        public HotkeyConfig WithAliases(params IDictionary<string, string>[] tempAliasesDics)
        {
            _tempAliasesDics = tempAliasesDics;
            return this;
        }
    }
}