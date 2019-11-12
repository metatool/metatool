using System.Runtime.InteropServices.WindowsRuntime;
using Metatool.Command;
using Metatool.Service;

namespace Metatool.Input
{
    partial class Keyboard
    {
        public IKeyCommand GetToken(ICommandToken<IKeyEventArgs> commandToken,
            IKeyboardCommandTrigger trigger) => new KeyCommandToken(commandToken, trigger);

        public IToggleKey GeToggleKey(Key key) => new ToggleKey(key);
    
    }
}
