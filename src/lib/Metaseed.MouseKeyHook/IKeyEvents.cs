using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaseed.Input
{
    public interface IKeyEvents
    {
        void Hit(string actionId, string description, Action<KeyEventArgsExt> action);
    }
}
