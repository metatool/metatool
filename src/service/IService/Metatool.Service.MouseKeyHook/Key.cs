using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            // if (Codes.Contains(AnyKeyCode))
            // {
            //     Codes.Clear();
            //     Codes.Add(AnyKeyCode);
            // }

        }
        public Key(params Key[] keys)
        {
            Codes = new SortedSet<Keys>(keys.SelectMany(k=>k.Codes));
            // if (Codes.Contains(AnyKeyCode))
            // {
            //     Codes.Clear();
            //     Codes.Add(AnyKeyCode);
            // }
        }



        public override string ToString()
        {
            return  $"({string.Join("|", Codes)})";
        }

    }
}
