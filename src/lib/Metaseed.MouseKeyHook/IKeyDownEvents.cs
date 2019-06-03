using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaseed.Input
{
    public interface IKeyDownEvents
    {
        void Down(string actionId, string description, Action<KeyEventArgsExt> action);
    }
}
