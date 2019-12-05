using System.Collections.Generic;

namespace Metatool.Service
{
    public class KeyboardConfig
    {
        public OrderedDictionary<string, string> KeyAliases { get; set; }
        public int RepeatDelay { get; set; } = 3000;
        public OrderedDictionary<string, HotStringDef> HotStrings { get; set; }
    }

    public class InputServiceConfig
    {
        public KeyboardConfig Keyboard { get; set; }

    }
    public class ServicesConfig
    {
       public  InputServiceConfig  Input { get; set; }
    }

    public class MetatoolConfig
    {
        public ServicesConfig Services { get; set; }
    }
}
