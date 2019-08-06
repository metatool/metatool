using Metaseed.Input.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using Metaseed.DataStructures;
using Metaseed.Input.MouseKeyHook.Implementation;
using OneOf;

namespace Metaseed.Input
{
    using Hotkey = OneOf<ISequenceUnit, ISequence>;

    public interface IMeta
    {
        string Id { get; }
        bool Disable { get; set; }
    }

    public class Meta
    {
        public string Id { get; set; }
        public bool Disable { get; set; }
    }


    public interface IMetaKey : IMeta,IRemove
    {
        Hotkey Hotkey { get; set; }
    }

    public class HotkeyToken : IRemoveChangeable<Hotkey>
    {
        private readonly   ITrie<ICombination, KeyEventAction> _trie;
        internal readonly IList<ICombination>                 _hotkey;
        internal readonly   KeyEventAction                      _action;

        public HotkeyToken(ITrie<ICombination, KeyEventAction> trie, IList<ICombination> hotkey,
            KeyEventAction action)
        {
            _trie         = trie;
            _hotkey = hotkey;
            _action       = action;
        }

        public void Remove()
        {
            var r = _trie.Remove(_hotkey, action => action.Equals(_action));
            Console.WriteLine(r);
        }

        public bool Change(Hotkey key)
        {
            ((IRemove) this).Remove();
            key.Switch(k => _trie.Add(k.ToCombination(), _action),
                s => _trie.Add(s.ToList(), _action));
            return true;
        }
    }

    public class MetaKey : IMetaKey
    {
        private readonly HotkeyToken _token;
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

        public void Remove()
        {
            _token.Remove();
        }


        public MetaKey(ITrie<ICombination, KeyEventAction> trie, IList<ICombination> combinations,
            KeyEventAction action)
        {
            _token = new HotkeyToken(trie, combinations, action);

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
        public string Id { get;  }

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
        public void Remove()
        {
            this.ForEach(k => k.Remove());
        }
    }

    public class MetaPackage : IMetaPackage
    {
        protected IEnumerable<(FieldInfo, MetaKey)> GetMetas()
        {
            var commands = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => typeof(IMetaKey).IsAssignableFrom(f.FieldType))
                .Select(fi => (fi, fi.GetValue(this) as MetaKey));
            return commands;
        }

        public virtual void Start()
        {
            GetMetas().ToList().ForEach(c =>
            {
                var (fi, metaKey) = c;
                if (string.IsNullOrEmpty(metaKey.Id))
                    metaKey.Id = GetType().FullName + fi.Name;
            });
        }
    }

    public class KeyMetaPackage : MetaPackage
    {
    }
}