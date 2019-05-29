﻿using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface ISequence : IKeyEvents
    {
        ISequence Then(Keys key);
        ISequence Then(ICombination combination);
    }

}