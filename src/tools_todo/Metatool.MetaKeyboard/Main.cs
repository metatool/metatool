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
            var keyConfig    = new KeyboardConfig();
            var keyboard61   = new Keyboard61();
            var mouse        = Services.GetOrCreate<MouseViaKeyboard>();
            var fun          = new FunctionalKeys();
            var fileExplorer = Services.GetOrCreate<FileExplorer>();
            var hotstrings   = new HostStrings();
            var windowKeys   = new WindowKeys();
            var software     = Services.GetOrCreate<Software>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IKeyboard keyboard, IMouse mouse, IConfig<Config> config)
        {
            _keyboard      = keyboard;
            _mouse         = mouse;
            Config.Current = config.CurrentValue;
            RegisterCommands();
        }
    }
}