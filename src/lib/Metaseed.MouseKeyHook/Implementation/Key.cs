using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Metaseed.Input.MouseKeyHook.Implementation
{
    public class Key: IKey
    {

        public Key(Keys key, KeyEventType type)
        {
            TriggerKey = key;
            Type = type;
        }


        public override string ToString()
        {
            return TriggerKey + ": " + Type;
        }

        public override int GetHashCode()
        {
            return (int)TriggerKey | (int)Type << 24;
        }

        protected bool Equals(Key other)
        {
            return TriggerKey == other.TriggerKey && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Key)obj);
        }

        public Keys TriggerKey { get; }
        public KeyEventType Type { get; }
    }
}
