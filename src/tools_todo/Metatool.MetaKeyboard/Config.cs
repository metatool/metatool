using System.Collections.Generic;
using System.IO;
using Metatool.Service;
using Metatool.UI;

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

    public class Keyboard61Package
    {

        public Dictionary<string,string> KeyMaps { get; set; }
    }

    public class Tools
    {
        public string Everything            { get; set; }
        public string GifTool               { get; set; }
        public string SearchEngine          { get; set; }
        public string SearchEngineSecondary { get; set; }
        public string Code                  { get; set; }
        public string Editor                { get; set; }
        public string Terminal              { get; set; }
        public string Ruler                 { get; set; }
        public string VisualStudio          { get; set; }
        public string ProcessExplorer       { get; set; }
        public string VisualMachineManager  { get; set; }

        public string Inspect { get; set; }
    }

    public class FileExplorerPackage
    {
        public FileExplorerHotKeys HotKeys { get; set; }
        
    }

    public class SoftwarePackage
    {
        public SoftwareHotKeys HotKeys { get; set; }
    }

    public class FileExplorerHotKeys
    {
        public HotkeyConfig FocusItemsView { get; set; }
        public HotkeyConfig FocusNavigationTreeView { get; set; }
        public HotkeyConfig CopySelectedPath { get; set; }
        public HotkeyConfig NewFile { get; set; }
        public HotkeyConfig ShowDesktopFolder { get; set; }

    }

    public class SoftwareHotKeys
    {
        public HotkeyConfig DoublePinyinSwitch { get; set; }
        public HotkeyConfig  Find { get; set; }
        public HotkeyConfig OpenTerminal { get; set; }
        public HotkeyConfig OpenCodeEditor { get; set; }
        public HotkeyConfig WebSearch { get; set; }
        public HotkeyConfig StartTaskExplorer { get; set; }
    }

    [ToolConfig]
    public class Config
    {
        private static Config _config;
        private Tools _tools;

        public static Config Current
        {
            internal set => _config = value;
            get => _config;
        }

        public Dictionary<string, string> KeyAliases { get; set; }
        public Keyboard61Package Keyboard61Package { get; set; }

        public FileExplorerPackage FileExplorerPackage { get; set; }

        public SoftwarePackage SoftwarePackage { get; set; }

        public Settings  Settings  { get; set; }

        public Tools Tools
        {
            get => _tools;
            set
            {
                var baseDir = Context.ToolDir<KeyboardTool>();

                foreach (var info in value.GetType().GetProperties())
                {
                    if (info.GetValue(value) is string v && v.StartsWith('.'))
                    {
                        var abs = Path.GetFullPath(Path.Combine(baseDir, v));
                        info.SetValue(value, abs);
                    }
                }

                _tools = value;
            }
        }

        public Registers Registers { get; set; }
    }
}
