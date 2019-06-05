using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public class ToggleKeys
    {
        public static ToggleKeys NumLock = new ToggleKeys(Keys.NumLock);
        public static ToggleKeys CapsLock = new ToggleKeys(Keys.CapsLock);
        public static ToggleKeys ScrollLock = new ToggleKeys(Keys.Scroll);
        public static ToggleKeys Insert = new ToggleKeys(Keys.Insert);

        private readonly Keys _key;

        public ToggleKeys(Keys key)
        {
            _key = key;
        }

        public void On()
        {
            Control.IsKeyLocked()
        }

    }
}
