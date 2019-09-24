using System.Windows.Forms;
using Metatool.Input.MouseKeyHook;

namespace Metatool.Input
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
