using Metatool.Service;

namespace Metatool.Tools.WinShell
{
    public class FileExplorerPackage
    {
        public FileExplorerHotKeys Hotkeys { get; set; }
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

    public class TaskViewPackage
    {
        public TaskViewHotKeys Hotkeys { get; set; }
    }

    public class TaskViewHotKeys
    {
        public HotkeyTrigger TaskViewNextScreen { get; set; }
        public HotkeyTrigger TaskViewPreviousScreen { get; set; }
    }

    [ToolConfig]
    public class Config
    {
        public FileExplorerPackage FileExplorerPackage { get; set; }
        public TaskViewPackage TaskViewPackage { get; set; }
    }
}
