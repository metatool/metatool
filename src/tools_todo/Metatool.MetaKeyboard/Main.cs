using ConsoleApp1;
using Metatool.Service;

namespace Metatool.MetaKeyboard
{
    public class KeyboardTool : ToolBase
    {
        private readonly IKeyboard _keyboard;
        private readonly IMouse    _mouse;

        public override bool OnLoaded()
        {
            var keyboard61 = Services.GetOrCreate<Keyboard61>();
            var mouse        = Services.GetOrCreate<KeyboardMouse>();
            var fun          = new FunctionalKeys();
            var fileExplorer = Services.GetOrCreate<FileExplorer>();
            var hotstrings   = Services.GetOrCreate<HotStrings>();
            var software     = Services.GetOrCreate<Software>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IKeyboard keyboard, IMouse mouse, IConfig<Config> config)
        {
            _keyboard      = keyboard;
            _mouse         = mouse;
            RegisterCommands();


        }

      
    }
}