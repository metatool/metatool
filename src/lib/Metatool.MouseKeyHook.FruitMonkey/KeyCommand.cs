using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Input.MouseKeyHook.Implementation;

public class KeyCommand : Command<IKeyEventArgs>
{
	public string Id { get; set; }

    public KeyCommand(Action<IKeyEventArgs> action) => Execute = action;
}
