using Metatool.Service;
using System.Collections.Generic;
using Metatool.Service.ScreenHint;

namespace Metatool.Tools.KeyMouse
{
    public class MouseKeyboardHotKeys
    {
        public HotkeyTrigger MouseToFocus { get; set; }
        public HotkeyTrigger MouseScrollUp { get; set; }
        public HotkeyTrigger MouseScrollDown { get; set; }
        public HotkeyTrigger MouseLeft { get; set; }
        public HotkeyTrigger MouseRight { get; set; }
        public HotkeyTrigger MouseUp { get; set; }
        public HotkeyTrigger MouseDown { get; set; }
        public HotkeyTrigger MouseLeftClick { get; set; }
        public HotkeyTrigger MouseLeftClickAlt { get; set; }
        public HotkeyTrigger MouseLeftClickLast { get; set; }

    }

    public class KeyboardMousePackage
    {
        public int MouseMoveDelta { get; set; } = 10;
        public bool MouseFollowActiveWindow { get; set; }
        public  ScreenHintConfig ScreenHintConfig {get; set;}
        public IDictionary<string, KeyMapDef> KeyMaps { get; set; }

        public MouseKeyboardHotKeys Hotkeys { get; set; }
    }

    [ToolConfig]
    public class KeyMousePluginConfig
    {
        public KeyboardMousePackage KeyboardMousePackage { get; set; }
    }
}
