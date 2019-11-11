using System.IO;
using Metatool.Plugin;

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
        public string Everything            { get; set; }
        public string GifTool               { get; set; }
        public string SearchEngine          { get; set; }
        public string SearchEngineSecondary { get; set; }
        public string Code                  { get; set; }
        public string Editor                { get; set; }
        public string Cmd                   { get; set; }
        public string Ruler                 { get; set; }
        public string VisualStudio          { get; set; }
        public string ProcessExplorer       { get; set; }
        public string VisualMachineManager  { get; set; }

        public string Inspect { get; set; }
    }

    [ToolConfig]
    public class Config
    {
        private static Config _config;

        public static Config Current
        {
            internal set
            {
                _config = value;
                var tools = _config.Tools;

                var baseDir = Context.ToolDir<KeyboardTool>();

                foreach (var info in tools.GetType().GetFields())
                {
                    if (info.GetValue(tools) is string v && v.StartsWith('.'))
                    {
                        var abs = Path.GetFullPath(Path.Combine(baseDir, v));
                        info.SetValue(tools, abs);
                    }
                }
            }
            get => _config;
        }

        public Settings  Settings  { get; set; }
        public Tools     Tools     { get; set; }
        public Registers Registers { get; set; }
    }
}