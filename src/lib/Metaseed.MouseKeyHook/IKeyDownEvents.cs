using System;

namespace Metaseed.Input
{
    public interface IKeyDownEvents
    {
        void Down(string actionId, string description, Action<KeyEventArgsExt> action);
    }
}
