using System.Collections.Generic;
using System.IO;
using Metatool.Service;
using Metatool.Service.Keyboard;
using Metatool.Tools.MetaKeyboard;

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

    public class KeyboardPackage
    {
        public Keyboard61HotKeys Hotkeys { get; set; }
        public Dictionary<string, KeyMapDef> KeyMaps { get; set; }
    }

    public class Keyboard61HotKeys
    {
        public HotkeyTrigger ToggleCaps { get; set; }
    }

    public class KeyboardMousePackage
    {
        public bool MouseFollowActiveWindow { get; set; }

        public IDictionary<string, KeyMapDef> KeyMaps { get; set; }

        public MouseKeyboardHotKeys Hotkeys { get; set; }
    }

    public class MouseKeyboardHotKeys
    {
        public HotkeyTrigger MouseToFocus { get; set; }
        public HotkeyTrigger MouseScrollUp { get; set; }
        public HotkeyTrigger MouseScrollDown { get; set; }
        public HotkeyTrigger MouseLeftClick { get; set; }
        public HotkeyTrigger MouseLeftClickLast { get; set; }
    }

    public class SoftwarePaths
    {
        public string Everything { get; set; }
        public string GifTool { get; set; }
        public string SearchEngine { get; set; }
        public string SearchEngineSecondary { get; set; }
        public string Code { get; set; }
        public string Editor { get; set; }
        public string Terminal { get; set; }
        public string Ruler { get; set; }
        public string VisualStudio { get; set; }
        public string ProcessExplorer { get; set; }
        public string VisualMachineManager { get; set; }

        public string Inspect { get; set; }
    }

    public class FileExplorerPackage
    {
        public FileExplorerHotKeys Hotkeys { get; set; }
    }

    public class SoftwarePackage
    {
        public OrderedDictionary<string, string> KeyAliases { get; set; }

        public SoftwareHotKeys Hotkeys { get; set; }
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
        public HotkeyTrigger FocusItemsView { get; set; }
        public HotkeyTrigger FocusNavigationTreeView { get; set; }
        public HotkeyTrigger CopySelectedPath { get; set; }
        public HotkeyTrigger NewFile { get; set; }
        public HotkeyTrigger ShowDesktopFolder { get; set; }
        public HotkeyTrigger PasteAsFile { get; set; }
    }

    public class SoftwareHotKeys
    {
        public HotkeyTrigger DoublePinyinSwitch { get; set; }
        public HotkeyTrigger Find { get; set; }
        public HotkeyTrigger OpenTerminal { get; set; }
        public HotkeyTrigger OpenCodeEditor { get; set; }
        public HotkeyTrigger WebSearch { get; set; }
        public HotkeyTrigger StartTaskExplorer { get; set; }
        public HotkeyTrigger OpenScreenRuler { get; set; }
        public HotkeyTrigger StartInspect { get; set; }
        public HotkeyTrigger StartNotepad { get; set; }
        public HotkeyTrigger StartVisualStudio { get; set; }
        public HotkeyTrigger StartGifRecord { get; set; }
        public HotkeyTrigger ToggleDictionary { get; set; }
    }

    [ToolConfig]
    public class Config
    {
        public OrderedDictionary<string, string> KeyAliases { get; set; }
        public ContextHotkey<string> SpecialFrenchChars { get; set; }
        public KeyboardPackage KeyboardPackage { get; set; }

        public FileExplorerPackage FileExplorerPackage { get; set; }
        public KeyboardMousePackage KeyboardMousePackage { get; set; }

        public SoftwarePackage SoftwarePackage { get; set; }

        public Settings Settings { get; set; }

        public Registers Registers { get; set; }
    }
}