using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Metatool.Service
{
    /// <summary>
    /// https://www.w3.org/TR/uievents-key/
    /// </summary>
    // [DebuggerDisplay("{this}")]
    public partial class Key : IComparable, IComparable<Key>, ISequenceUnit, ISequencable
    {
        private SortedSet<Keys> _codes;
        private int             _val;

        public SortedSet<Keys> Codes
        {
            get => _codes;
            private set
            {
                _codes = value;
                _val   = value.Aggregate<Keys, int>(0, (o, c1) => o + (int) c1);
            }
        }

        public Key(params Keys[] keyCodes)
        {
            Codes = new SortedSet<Keys>(keyCodes);
        }
        public Key(params Key[] keys)
        {
            Codes = new SortedSet<Keys>(keys.SelectMany(k=>k.Codes));
        }

        public static Key Parse(string str)
        {
            var keys = str.Split("|", StringSplitOptions.RemoveEmptyEntries).Select(s =>
            {
                var keyName = s.Trim().ToUpper();
                var r       = All.TryGetValue(keyName, out var key);
                if (r) return key;

                throw new Exception($"Could not parse Key {str}");
            }).ToArray();

            return new Key(keys);
        }
        public static bool TryParse(string str, out Key value)
        {
            try
            {
                value = Parse(str);
                return true;
            }
            catch (Exception e)
            {
                Services.CommonLogger?.LogError(e.Message + $"{Environment.NewLine} Please use the following Keys: {string.Join(",",AllNames)}");
                value = null;
                return false;
            }
        }

        public override string ToString()
        {
            return  $"{string.Join("|", Codes)}";
        }

    }
}
