using System.Windows.Forms;

namespace Metatool.Input
{
    public interface ISequencable 
    {
        ISequence Then(Keys key);
        ISequence Then(ISequenceUnit sequencable);

    }

}
