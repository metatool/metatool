using System.Collections.Generic;

namespace Metatool.Service
{
    public class KeyboardConfig
    {
        public IDictionary<string, string> KeyAliases { get; set; }
        public int RepeatDelay { get; set; } = 3000;

    }

    public class InputConfig
    {
        public KeyboardConfig Keyboard { get; set; }

    }
    public class ServicesConfig
    {
       public  InputConfig  Input { get; set; }
    }

    public class MetatoolConfig
    {
        public ServicesConfig Services { get; set; }
    }
}
