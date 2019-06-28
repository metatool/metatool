using System;
using System.IO;
using Newtonsoft.Json;

namespace Metaseed.MetaKeyboard
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
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@".\config.json"));
                var tools = _config.Tools;

                void ToAbsPath(ref string relativePath)
                {
                    if (relativePath.StartsWith('.'))
                    {
                        relativePath = Path.Combine(baseDir, relativePath);
                    }
                }

                ToAbsPath(ref tools.VisualStudio);
                ToAbsPath(ref tools.Code);
                return _config;
            }
        }

        public Settings  Settings;
        public Tools     Tools;
        public Registers Registers;
    }
}