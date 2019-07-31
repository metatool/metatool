using System.Windows.Forms;
using Metaseed.Input.MouseKeyHook;

namespace Metaseed.Input
{
    public partial class Combination
    {
        public ISequence Then(ISequencable sequencable)
        {
            return new Sequence(this, (Combination)sequencable);
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }
    }
}