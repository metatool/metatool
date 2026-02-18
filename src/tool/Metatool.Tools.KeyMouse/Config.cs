using Metatool.Service;

namespace Metatool.Tools.KeyMouse
{
    public class KeyMouseHotKeys
    {
        public HotkeyTrigger Activate { get; set; }
        public HotkeyTrigger Reshow { get; set; }
    }

    [ToolConfig]
    public class PluginConfig
    {
        public string Keys { get; set; } = "ASDFQWERZXCVTGBHJKLYUIOPNM";
        public KeyMouseHotKeys Hotkeys { get; set; }
    }
}
