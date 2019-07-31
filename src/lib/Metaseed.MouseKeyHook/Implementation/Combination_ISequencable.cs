using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;

namespace Metaseed.Input
{
    public partial class Combination
    {
        public ISequence Then(ISequenceUnit sequencable)
        {
            return new Sequence(this, sequencable.ToCombination());
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }
    }
}