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

            var conf = Config.Current;
            var aliases = conf.KeyAliases;
            var maps = conf.KeyMaps;
            RegisterKeyMaps(maps, aliases);
        }

        private void RegisterKeyMaps(Dictionary<string, string> maps, Dictionary<string, string> aliases)
        {
            static string ReplaceAlias(string v, Dictionary<string, string> aliases)
            {
                foreach (var alias in aliases)
                {
                    v = v.Replace(alias.Key, alias.Value);
                }

                return v;
            }

            foreach (var map in maps)
            {
                try
                {
                    var source = ReplaceAlias(map.Key, aliases);
                    var target = ReplaceAlias(map.Value, aliases);
                    var s      = Sequence.Parse(source);
                    var t      = Combination.Parse(target);
                    s.Map(t);
                }
                catch (Exception e)
                {
                    Logger.LogError("KeyMappings: " + e.Message);
                }
            }
        }
    }
}