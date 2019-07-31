using System;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public partial class Key
    {
        public ISequence Then(Keys key) => new Combination(this).Then(key);

        public ISequence Then(ISequenceUnit sequencable) => new Combination(this).Then(sequencable);
    }
}