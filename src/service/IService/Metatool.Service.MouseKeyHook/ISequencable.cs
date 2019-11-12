using System.Windows.Forms;

namespace Metatool.Service
{
    public interface ISequencable 
    {
        ISequence Then(Keys key);
        ISequence Then(ISequenceUnit sequencable);
    }

}
