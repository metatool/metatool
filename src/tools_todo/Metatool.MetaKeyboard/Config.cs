using System.Collections.Generic;
using System.IO;
using Metatool.Service;

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

    public class KeyboardMousePackage
    {
        public Dictionary<string, string> KeyMaps { get; set; }

        public MouseKeyboardHotKeys HotKeys { get; set; }
    }

    public class MouseKeyboardHotKeys
    {
        public HotkeyConfig MouseToFocus { get; set; }
        public HotkeyConfig MouseScrollUp { get; set; }
        public HotkeyConfig MouseScrollDown { get; set; }
        public HotkeyConfig MouseLeftClick { get; set; }
        public HotkeyConfig MouseLeftClickLast { get; set; }

    }

    public class SoftwarePaths
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
        public IDictionary<string,string> KeyAliases { get; set; }
        public SoftwareHotKeys HotKeys { get; set; }
        private SoftwarePaths _softwarePaths;

        public SoftwarePaths SoftwarePaths
        {
            get => _softwarePaths;
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

                _softwarePaths = value;
            }
        }
    }

    public class FileExplorerHotKeys
    {
        public HotkeyConfig FocusItemsView { get; set; }
        public HotkeyConfig FocusNavigationTreeView { get; set; }
        public HotkeyConfig CopySelectedPath { get; set; }
        public HotkeyConfig NewFile { get; set; }
        public HotkeyConfig ShowDesktopFolder { get; set; }

    }

    public class HotStringPackage
    {
        public IDictionary<string, string[]> HotStrings { get; set; }
    }

    public class SoftwareHotKeys
    {
        public HotkeyConfig DoublePinyinSwitch { get; set; }
        public HotkeyConfig  Find { get; set; }
        public HotkeyConfig OpenTerminal { get; set; }
        public HotkeyConfig OpenCodeEditor { get; set; }
        public HotkeyConfig WebSearch { get; set; }
        public HotkeyConfig StartTaskExplorer { get; set; }
        public HotkeyConfig OpenScreenRuler { get; set; }
        public HotkeyConfig StartInspect { get; set; }
        public HotkeyConfig StartNotepad { get; set; }
        public HotkeyConfig StartVisualStudio { get; set; }
        public HotkeyConfig StartGifRecord { get; set; }
        public HotkeyConfig ToggleDictionary { get; set; }

    }

    [ToolConfig]
    public class Config
    {
        public Keyboard61Package Keyboard61Package { get; set; }

        public FileExplorerPackage FileExplorerPackage { get; set; }
        public KeyboardMousePackage KeyboardMousePackage { get; set; }

        public HotStringPackage HotStringPackage { get; set; }
        public SoftwarePackage SoftwarePackage { get; set; }

        public Settings  Settings  { get; set; }

        public Registers Registers { get; set; }
    }
}
