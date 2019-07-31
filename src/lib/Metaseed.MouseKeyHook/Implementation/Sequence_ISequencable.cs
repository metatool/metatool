using System.Linq;
using System.Windows.Forms;

namespace Metaseed.Input.MouseKeyHook
{
    public partial class Sequence
    {
        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }

        public ISequence Then(ISequenceUnit sequencable)
        {
            this.Append(sequencable.ToCombination());
            return this;
        }
    }
}