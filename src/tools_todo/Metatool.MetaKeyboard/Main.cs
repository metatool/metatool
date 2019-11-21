using ConsoleApp1;
using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class KeyboardTool : ToolBase
    {
        public override bool OnLoaded()
        {
            var keyboard61   = Services.GetOrCreate<Keyboard61>();
            var mouse        = Services.GetOrCreate<KeyboardMouse>();
            var fun          = new FunctionalKeys();
            var fileExplorer = Services.GetOrCreate<FileExplorer>();
            var hotstrings   = Services.GetOrCreate<HotStrings>();
            var software     = Services.GetOrCreate<Software>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IConfig<Config> config)
        {
            RegisterCommands();
        }
    }
}