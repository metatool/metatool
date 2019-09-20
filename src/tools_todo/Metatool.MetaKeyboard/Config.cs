using System;
using System.IO;
using Newtonsoft.Json;

namespace Metatool.MetaKeyboard
{
    public class Settings
    {
        public string RemoteDesktopEnable;
    }

    public class Registers
    {
        public string WorkDir;
    }

    public class Tools
    {
        public string EveryThing;
        public string GifTool;
        public string SearchEngine;
        public string SearchEngineSecondary;
        public string Code;
        public string Editor;
        public string Cmd;
        public string Ruler;
        public string VisualStudio;
        public string ProcessExplorer;
        public string VisualMachineManager;
        public string Inspect;
    }

    public class Config
    {
        private static Config _config;

        public static Config Current
        {
            get
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                if (_config != null) return _config;
                var configPath = Path.Combine(Path.GetDirectoryName(typeof(Config).Assembly.Location), @".\config.json");
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
                var tools = _config.Tools;

                foreach (var info in tools.GetType().GetFields())
                {
                    if (info.GetValue(tools) is string v && v.StartsWith('.'))
                    {
                        var abs = Path.GetFullPath( Path.Combine(baseDir,v));
                        info.SetValue(tools,abs);
                    }
                }

                return _config;
            }
        }

        public Settings  Settings;
        public Tools     Tools;
        public Registers Registers;
    }
}
