using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Metatool.MetaKeyboard
{
    public class Settings
    {
        public bool RemoteDesktopEnable { get; set; }
    }

    public class Registers
    {
        public string WorkDir { get; set; }
    }

    public class Tools
    {
        public string Everything { get; set; }
        public string GifTool { get; set; }
        public string SearchEngine { get; set; }
        public string SearchEngineSecondary { get; set; }
        public string Code { get; set; }
        public string Editor { get; set; }
        public string Cmd { get; set; }
        public string Ruler { get; set; }
        public string VisualStudio { get; set; }
        public string ProcessExplorer { get; set; }
        public string VisualMachineManager { get; set; }

        public string Inspect { get; set; }
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
                var config = File.ReadAllText(configPath);
                _config = JsonSerializer.Deserialize<Config>(config, new JsonSerializerOptions(){ReadCommentHandling = JsonCommentHandling.Skip});
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

        public Settings Settings { get; set; }
        public Tools Tools { get; set; }
        public Registers Registers { get; set; }
    }
}
