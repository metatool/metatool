using System.Collections.Generic;
using System.Windows.Forms;

namespace Metatool.Service
{
    public partial class Combination
    {
        public ISequence Then(IHotkey hotkey)
        {
            var sequence = new Sequence(this);
            sequence.AddRange(hotkey.ToSequence());
            return sequence;
        }

        public ISequence Then(Keys key)
        {
            return this.Then(new Combination(key));
        }
        public ISequence ToSequence() => new Sequence(this);
    }
}