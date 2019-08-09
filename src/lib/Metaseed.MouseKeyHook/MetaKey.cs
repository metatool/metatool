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
    }

    public class HotkeyToken : IRemoveChangeable<Hotkey>
    {
        private readonly  ITrie<ICombination, KeyEventAction> _trie;
        internal readonly IList<ICombination>                 _hotkey;
        internal readonly KeyEventAction                      _action;

        public HotkeyToken(ITrie<ICombination, KeyEventAction> trie, IList<ICombination> hotkey,
            KeyEventAction action)
        {
            _trie   = trie;
            _hotkey = hotkey;
            _action = action;
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

        public void Remove()
        {
            _token.Remove();
        }

        internal KeyEvent KeyEvent => _token._action.KeyEvent; 

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
        public string Id
        {
            get => this.Aggregate("",(a,c)=>a+c.Id);
            set
            {
                for (int i = 0; i < this.Count; i++)
                {
                    var k = (MetaKey)this[i];
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
                    metaKey.Id = GetType().FullName +"."+ fi.Name;
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
                    if (meta is MetaKey metaKey && string.IsNullOrEmpty(metaKey._token._action.Command.Id))
                        metaKey._token._action.Command.Id = metaKey.Id;
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