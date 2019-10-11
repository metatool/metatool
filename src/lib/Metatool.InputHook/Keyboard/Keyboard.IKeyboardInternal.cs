using System.Runtime.InteropServices.WindowsRuntime;
using Metatool.Command;

namespace Metatool.Input
{
    partial class Keyboard
    {
        public IKeyToken GetToken(ICommandToken<IKeyEventArgs> commandToken,
            IKeyboardCommandTrigger trigger) => new KeyToken(commandToken, trigger);

        public IToggleKey GeToggleKey(Key key) => new ToggleKey(key);
    
    }
}
