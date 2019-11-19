using System;
using System.Collections.Generic;
using System.Windows.Markup;
using ConsoleApp1;
using Metatool.Service;
using Microsoft.Extensions.Logging;

namespace Metatool.MetaKeyboard
{
    public class KeyboardTool : ToolBase
    {
        private readonly IKeyboard _keyboard;
        private readonly IMouse    _mouse;

        public override bool OnLoaded()
        {
            var keyConfig    = new KeyboardConfig();
            var keyboard61 = Services.GetOrCreate<Keyboard61>();
            var mouse        = Services.GetOrCreate<MouseViaKeyboard>();
            var fun          = new FunctionalKeys();
            var fileExplorer = Services.GetOrCreate<FileExplorer>();
            var hotstrings   = new HostStrings();
            var software     = Services.GetOrCreate<Software>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IKeyboard keyboard, IMouse mouse, IConfig<Config> config)
        {
            _keyboard      = keyboard;
            _mouse         = mouse;
            Config.Current = config.CurrentValue;
            keyboard.AddAliases(config.CurrentValue.KeyAliases);
            RegisterCommands();


        }

      
    }
}