using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Metaseed.Input
{
    /// <summary>
    /// https://www.w3.org/TR/uievents-key/
    /// </summary>
    public partial class Key : IKey
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
            if (Codes.Contains(AnyKeyCode))
            {
                Codes.Clear();
                Codes.Add(AnyKeyCode);
            }

        }
        public Key(params Key[] keys)
        {
            Codes = new SortedSet<Keys>(keys.SelectMany(k=>k.Codes));
            if (Codes.Contains(AnyKeyCode))
            {
                Codes.Clear();
                Codes.Add(AnyKeyCode);
            }
        }



        public override string ToString()
        {
            if (Codes.Count == 1)
                return $"{Codes.First()}";
            return $"({string.Join("|", Codes)})";
        }


    }
}