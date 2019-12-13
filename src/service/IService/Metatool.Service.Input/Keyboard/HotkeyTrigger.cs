using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Metatool.Command;
using Microsoft.Extensions.Logging;

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
                string str => new HotkeyTrigger() {Hotkey = str},
                _ => base.ConvertFrom(context, culture, value)
            };
        }
    }

    public interface IHotkeyTrigger
    {
        string                      Hotkey  { get; set; }
        IHotkey                     Key     { get; }
        KeyEvent                    Event   { get; set; }
        string                      Tree    { get; set; }
        bool                        Handled { get; set; }
        bool                        Enabled { get; set; }
        IDictionary<string, string> Context { get; set; }
        HotkeyTrigger WithAliases(params IDictionary<string, string>[] tempAliasesDics);

        IKeyCommand OnEvent(Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null);

        IKeyCommand MapOnHit(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null);

        IKeyCommand MapOnAllUp(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null);
    }

    [TypeConverter(typeof(AliasedSequenceTriggerConverter))]
    public class HotkeyTrigger : IHotkeyTrigger
    {
        public HotkeyTrigger()
        {
        }

        public HotkeyTrigger(IHotkey hotKey)
        {
            _trigger = hotKey;
        }

        private static IKeyboard _keyboard;

        private static IKeyboard Keyboard =>
            _keyboard ??= Services.Get<IKeyboard>();

        public IHotkey Key
        {
            get
            {
                if (string.IsNullOrEmpty(Hotkey)) return _trigger;
                var hotkey = Keyboard.ReplaceAlias(Hotkey, _tempAliasesDics);
                _trigger = Sequence.Parse(hotkey);
                return _trigger;
            }
        }

        public bool   Handled { get; set; }
        public string Hotkey  { get; set; }

        public KeyEvent Event       { get; set; } = KeyEvent.Down;
        public string   Description { get; set; }

        public string                      Tree    { get; set; } = KeyStateTrees.Default;
        public bool                        Enabled { get; set; } = true;
        public IDictionary<string, string> Context { get; set; }

        public IKeyCommand OnEvent(Action<IKeyEventArgs> execute,
            Predicate<IKeyEventArgs> canExecute = null)
        {
            var trigger = Keyboard.OnEvent(Key, Event, Tree);
            var token   = trigger.Register(execute, canExecute, Description);
            return token;
        }

        public IKeyCommand MapOnHit(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            return Keyboard.MapOnHit(Key, target, predicate);
        }

        public IKeyCommand MapOnAllUp(ISequenceUnit target,
            Predicate<IKeyEventArgs> predicate = null)
        {
            return Keyboard.MapOnHitAndAllUp(Key, target, predicate);
        }

        private IDictionary<string, string>[] _tempAliasesDics;
        private IHotkey                       _trigger;

        public HotkeyTrigger WithAliases(params IDictionary<string, string>[] tempAliasesDics)
        {
            _tempAliasesDics = tempAliasesDics;
            return this;
        }

        /// <summary>
        /// 1. Abc => A
        /// 2. A&Bc => B
        /// 3. Abc#A+B,C$Handled=true, Enabled=true, Description=ab cs ef
        /// 4. ABC#$Handled=true
        /// 5. ABC#$$a=b,c=d
        /// </summary>
        /// <param name="hotkeyTrigger"></param>
        /// <returns></returns>
        public static IHotkeyTrigger Parse(string hotkeyTrigger)
        {
            var iNum  = hotkeyTrigger.LastIndexOf('#');
            var count = iNum == -1 ? hotkeyTrigger.Length : iNum;

            var hotChar = hotkeyTrigger[0];
            var des     = hotkeyTrigger;
            var iAnd    = hotkeyTrigger.IndexOf('&', 0, count);
            if (iAnd != -1)
            {
                hotChar = hotkeyTrigger[iAnd + 1];
                des     = hotkeyTrigger.Remove(iAnd, 1);
            }

            var trigger = new HotkeyTrigger() {Hotkey = hotChar.ToString().ToUpper(), Description = des};

            if (iNum != -1)
            {
                var keyWithProperties = hotkeyTrigger.Substring(iNum + 1).Split('$').ToList();
                if (!string.IsNullOrEmpty(keyWithProperties[0]))
                    trigger.Hotkey = keyWithProperties[0];
                keyWithProperties.RemoveAt(0);
                var properties = keyWithProperties.Select(p =>
                {
                    var d = new Dictionary<string, string>();
                    if (string.IsNullOrEmpty(p)) return d;
                    var entries = p.Split(',');
                    foreach (var entry in entries)
                    {
                        var pair = entry.Split('=');
                        if (pair.Length != 2 || string.IsNullOrEmpty(pair[0]) || string.IsNullOrEmpty(pair[1]))
                        {
                            Services.CommonLogger.LogError($"HotkeyTrigger Parse: format error - {entry}, in  {p}");
                        }

                        d.Add(pair[0], pair[1]);
                    }

                    return d;
                }).ToArray();


                if (properties.Length > 0)
                {
                    var prop = properties[0];
                    try
                    {
                        var d                                                      = nameof(HotkeyTrigger.Description);
                        if (prop.TryGetValue(d, out var desc)) trigger.Description = desc;
                        d = nameof(HotkeyTrigger.Enabled);
                        if (prop.TryGetValue(d, out desc)) trigger.Enabled = bool.Parse(desc);
                        d = nameof(HotkeyTrigger.Handled);
                        if (prop.TryGetValue(d, out desc)) trigger.Handled = bool.Parse(desc);
                        d = nameof(HotkeyTrigger.Event);
                        if (prop.TryGetValue(d, out desc)) trigger.Event = Enum.Parse<KeyEvent>(desc);
                        d = nameof(HotkeyTrigger.Tree);
                        if (prop.TryGetValue(d, out desc)) trigger.Tree = desc;
                        d = nameof(HotkeyTrigger.Hotkey);
                        if (prop.TryGetValue(d, out desc)) trigger.Hotkey = desc;
                    }
                    catch (Exception e)
                    {
                        Services.CommonLogger.LogError(e,
                            $"HotkeyTrigger Parse: cannot parse HotkeyTrigger properties + {e.Message}");
                    }
                }

                if (properties.Length > 1)
                {
                    trigger.Context = properties[1];
                }
            }

            return trigger;
        }
    }
}