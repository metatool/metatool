using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ISequence : IKeyState
    {
        ISequence Then(Keys key);
        ISequence Then(ICombination combination);
    }

}
