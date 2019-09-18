using System;
using System.Collections.Generic;
using System.Text;
using ConsoleApp1;
using Metaseed.Input;
using Metaseed.MetaKeyboard;
using Metaseed.MetaPlugin;
using Microsoft.Extensions.Logging;
using Mouse = Metaseed.MetaKeyboard.Mouse;

namespace Metaseed.ScreenHint
{
    public class KeyboardPlugin :PluginBase
    {

        public override bool Init()
        {
            var keyConfig    = new KeyboardConfig();
            var keyboard61   = new Keyboard61();
            var mouse        = new Mouse();
            var fun          = new FunctionalKeys();
            var fileExplorer = new FileExplorer();
            var hotstrings   = new HostStrings();
            var windowKeys   = new WindowKeys();
            var software = new Software();
            Keyboard.Hook();
            return base.Init();
        }

        public KeyboardPlugin(ILogger<KeyboardPlugin> logger) : base(logger)
        {
        }
    }
}
