using System;
using System.Collections.Generic;
using System.Text;
using ConsoleApp1;
using Metatool.Input;
using Metatool.MetaKeyboard;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;
using Mouse = Metatool.MetaKeyboard.Mouse;

namespace Metatool.ScreenHint
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
            Keyboard.Default.Hook();
            return base.Init();
        }

        public KeyboardPlugin(ILogger<KeyboardPlugin> logger) : base(logger)
        {
        }
    }
}
