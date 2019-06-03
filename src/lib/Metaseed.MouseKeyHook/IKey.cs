using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public interface IKey
    {
        Keys TriggerKey { get; }
        KeyEventType Type { get; }
    }
}
