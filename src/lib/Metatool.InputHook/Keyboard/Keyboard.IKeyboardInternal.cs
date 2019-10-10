using System.Runtime.InteropServices.WindowsRuntime;
using Metatool.Command;

namespace Metatool.Input
{
    partial class Keyboard
    {
        public IKeyboardCommandToken GetToken(ICommandToken<IKeyEventArgs> commandToken,
            IKeyboardCommandTrigger trigger) => new KeyboardCommandToken(commandToken, trigger);

        public IToggleKey GeToggleKey(Key key) => new ToggleKey(key);
    
    }
}
