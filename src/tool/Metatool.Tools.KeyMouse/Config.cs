using Metatool.Service;
using System.Collections.Generic;

namespace Metatool.Tools.KeyMouse
{
    public class MouseKeyboardHotKeys
    {
        public HotkeyTrigger MouseToFocus { get; set; }
        public HotkeyTrigger MouseScrollUp { get; set; }
        public HotkeyTrigger MouseScrollDown { get; set; }
        public HotkeyTrigger MouseLeftClick { get; set; }
        public HotkeyTrigger MouseLeftClickLast { get; set; }
    }
    public class KeyboardMousePackage
    {
        public bool MouseFollowActiveWindow { get; set; }
        public string Keys { get; set; } = "ASDFQWERZXCVTGBHJKLYUIOPNM";

        public IDictionary<string, KeyMapDef> KeyMaps { get; set; }

        public MouseKeyboardHotKeys Hotkeys { get; set; }
    }

    [ToolConfig]
    public class PluginConfig
    {
        public KeyboardMousePackage KeyboardMousePackage { get; set; }
    }
}
