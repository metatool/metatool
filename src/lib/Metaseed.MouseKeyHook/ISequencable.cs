using System.Collections.Generic;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ISequencable 
    {
        ISequence Then(Keys key);
        ISequence Then(ISequencable sequencable);
    }

}
