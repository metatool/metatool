using Metatool.Service;
using Metatool.Service.MouseKey;
using System.Diagnostics;

namespace Metatool.Input.MouseKeyHook.Implementation;

[DebuggerDisplay("{ToString()")]
public class KeyCommand : Command<IKeyEventArgs>
{
	public string Id { get; set; }

    public KeyCommand(Action<IKeyEventArgs> action) => Execute = action;
    public override string ToString()
    {
        return $"{{Description:{Description},Id:{Id},Disabled:{Disabled}}}";
    }
}
