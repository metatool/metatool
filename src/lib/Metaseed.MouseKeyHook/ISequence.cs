using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ISequence
    {
        ISequence Then(Keys key);
        ISequence Then(ICombination combination);
    }

}
