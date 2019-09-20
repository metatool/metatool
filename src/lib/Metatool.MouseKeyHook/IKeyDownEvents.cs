using System;

namespace Metatool.Input
{
    public interface IKeyDownEvents
    {
        void Down(string actionId, string description, Action<KeyEventArgsExt> action);
    }
}
