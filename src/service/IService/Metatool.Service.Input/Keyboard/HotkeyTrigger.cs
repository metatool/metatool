using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Metatool.Command;

namespace Metatool.Service
{
    public class AliasedSequenceTriggerConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            return value switch
            {
                string str =>new HotkeyTrigger() { HotKey = str},
                _ => base.ConvertFrom(context, culture, value)
            };
        }
    }

    [TypeConverter(typeof(AliasedSequenceTriggerConverter))]
    public class HotkeyTrigger
    {
        public HotkeyTrigger() { }

        public HotkeyTrigger(IHotkey hotKey)
        {
            _trigger = hotKey;
        }

        private static IKeyboard _keyboard;
        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();

        private static ICommandManager _commandManager;

        private static ICommandManager CommandManager =>
            _commandManager ??= Services.Get<ICommandManager>();

        private IHotkey Trigger
        {
            get
            {
                if (string.IsNullOrEmpty(HotKey)) return _trigger;
                var hotkey = Keyboard.ReplaceAlias(HotKey, _tempAliasesDics);
                _trigger = Sequence.Parse(hotkey);
                return _trigger;
            }
        }

        public string HotKey { get; set; }

        public KeyEvent KeyEvent { get; set; } = KeyEvent.Down;
        public string   Description { get; set; }
        public string   StateTree   { get; set; } = KeyStateTrees.Default;
        public bool     Enabled     { get; set; } = true;

        public IKeyCommand OnEvent(Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null)
        {
            var trigger = Keyboard.OnEvent(Trigger, KeyEvent, StateTree);
                                    var token   = trigger.Register(execute, canExecute, Description);
            return token;
        }

        public IKeyCommand MapOnHit(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null )
        {
            return Keyboard.MapOnHit(Trigger, target, predicate);
        }
        public IKeyCommand MapOnAllUp(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            return Keyboard.MapOnHitAndAllUp(Trigger, target, predicate);
        }
        private IDictionary<string, string>[] _tempAliasesDics;
        private IHotkey _trigger;

        public HotkeyTrigger WithAliases(IDictionary<string, string>[] tempAliasesDics)
        {
            _tempAliasesDics = tempAliasesDics;
            return this;
        }
    }
}