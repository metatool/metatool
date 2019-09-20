using Metatool.Input.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Metatool.DataStructures;
using Metatool.Input.MouseKeyHook.Implementation;
using OneOf;

namespace Metatool.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public interface IMeta
    {
        string Id      { get; set; }
        bool   Disable { get; set; }
    }

    public class Meta
    {
        public string Id      { get; set; }
        public bool   Disable { get; set; }
    }


    public interface IMetaKey : IMeta, IRemove
    {
        Hotkey Hotkey { get; set; }
        IMetaKey ChangeHotkey(Hotkey hotkey);
    }

    public class HotkeyToken : IRemoveChangeable<Hotkey>
    {
        private readonly  ITrie<ICombination, KeyEventCommand> _trie;
        internal readonly IList<ICombination>                  _hotkey;
        internal readonly KeyEventCommand                      Command;

        public HotkeyToken(ITrie<ICombination, KeyEventCommand> trie, IList<ICombination> hotkey,
            KeyEventCommand command)
        {
            _trie   = trie;
            _hotkey = hotkey;
            Command = command;
        }

        public void Remove()
        {
            var r = _trie.Remove(_hotkey, action => action.Equals(Command));
            Console.WriteLine(r);
        }

        public bool Change(Hotkey key)
        {
            ((IRemove) this).Remove();
            key.Switch(k => _trie.Add(k.ToCombination(), Command),
                s => _trie.Add(s.ToList(), Command));
            return true;
        }
    }

    public class MetaKey : IMetaKey
    {
        internal readonly HotkeyToken _token;

        public Hotkey Hotkey
        {
            get
            {
                if (_token._hotkey.Count > 1) return new Sequence(_token._hotkey.ToArray());
                var first = _token._hotkey.First();
                if (first.ChordLength > 0) return (first as Combination);

                return first.TriggerKey;
            }
            set => _token.Change(value);
        }

        public IMetaKey ChangeHotkey(Hotkey hotkey)
        {
            Hotkey = hotkey;
            return this;
        }

        public void Remove()
        {
            _token.Remove();
        }

        internal KeyEvent KeyEvent => _token.Command.KeyEvent;

        public MetaKey(ITrie<ICombination, KeyEventCommand> trie, IList<ICombination> combinations,
            KeyEventCommand command)
        {
            _token = new HotkeyToken(trie, combinations, command);
        }

        public string Id { get; set; }

        public bool Disable
        {
            get => _token._hotkey.Last().Disabled;
            set => _token._hotkey.Last().Disabled = value;
        }
    }

    public class MetaKeys : List<IMetaKey>, IMetaKey
    {
        public string Id
        {
            get => this.Aggregate("", (a, c) => a + c.Id);
            set
            {
                for (var i = 0; i < this.Count; i++)
                {
                    var k = (MetaKey) this[i];
                    k.Id = $"{value}_{i}-{k.KeyEvent}";
                }
            }
        }

        public bool Disable
        {
            get => this.First().Disable;
            set => this.ForEach(k => k.Disable = value);
        }

        public Hotkey Hotkey
        {
            get => this.First().Hotkey;
            set => this.ForEach(k => k.Hotkey = value);
        }

        public IMetaKey ChangeHotkey(Hotkey hotkey)
        {
            Hotkey = hotkey;
            return this;
        }

        public void Remove()
        {
            this.ForEach(k => k.Remove());
        }
    }

    public abstract class MetaPackage : IMetaPackage
    {
        protected IEnumerable<(FieldInfo, IMeta)> GetMetas()
        {
            var commands = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => typeof(IMeta).IsAssignableFrom(f.FieldType))
                .Select(fi => (fi, fi.GetValue(this) as IMeta));
            return commands;
        }

        public virtual void Start()
        {
            GetMetas().ToList().ForEach(c =>
            {
                var (fi, metaKey) = c;
                if (string.IsNullOrEmpty(metaKey.Id))
                    metaKey.Id = GetType().FullName + "." + fi.Name;
                else
                {
                    metaKey.Id = metaKey.Id;
                }
            });
        }
    }

    public class KeyMetaPackage : MetaPackage
    {
        public KeyMetaPackage()
        {
            Start();
        }

        public sealed override void Start()
        {
            base.Start();
            GetMetas().ToList().ForEach(c =>
            {
                void setId(IMetaKey meta)
                {
                    if (meta is MetaKey metaKey && string.IsNullOrEmpty(metaKey._token.Command.Command.Id))
                        metaKey._token.Command.Command.Id = metaKey.Id;
                }

                var (_, key) = c;
                switch (key)
                {
                    case MetaKey m:
                        setId(m);
                        break;
                    case System.Collections.Generic.List<IMetaKey> metaKeys:
                        metaKeys.ForEach(setId);
                        break;
                }
            });
        }
    }
}
