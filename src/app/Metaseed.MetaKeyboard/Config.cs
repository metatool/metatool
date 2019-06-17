using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
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
        Config() { }
        private static Config _config;
        public static Config Inst
        {
            get
            {
                if (_config != null) return _config;
                return _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@".\config.json"));
            }

        }
        public Settings Settings;
        public Tools Tools;
        public Registers Registers;

    }
}
