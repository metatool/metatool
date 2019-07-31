using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ICombinable
    {
        ICombination With(Keys chordKey);
        ICombination With(IEnumerable<Keys> keys);
        ICombination Control();
        ICombination Shift();
        ICombination Alt();
    }
}
