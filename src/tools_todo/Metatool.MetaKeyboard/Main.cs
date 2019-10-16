using ConsoleApp1;
using Metatool.Command;
using Metatool.Input;
using Metatool.MetaKeyboard;
using Metatool.Plugin;
using Mouse = Metatool.MetaKeyboard.Mouse;

namespace Metatool.ScreenHint
{
    public class KeyboardPlugin :PluginBase
    {

        public override bool OnLoaded()
        {
            var keyConfig    = new KeyboardConfig();
            var keyboard61   = new Keyboard61();
            var mouse        = new Mouse();
            var fun          = new FunctionalKeys();
            var fileExplorer = new FileExplorer();
            var hotstrings   = new HostStrings();
            var windowKeys   = new WindowKeys();
            var software = new Software();
            return base.OnLoaded();
        }

        public KeyboardPlugin(ICommandManager commandManager, IKeyboard keyboard)
        {
            // commandManager.Add(keyboard.Down(Caps + A), e =>
            // {
            //     
            //     logger.LogInformation("ssssss_______________");
            // });
        }
    }
}
