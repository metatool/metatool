using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Metatool.Input
{
    public interface IKeyboardState
    {
        bool IsDown(Key key);
        bool IsUp(Key key);
        bool IsOtherDown(Key key);
        bool AreAllDown(IEnumerable<Key> keys);
        bool AreAllUp(IEnumerable<Key> keys);
    }
}
