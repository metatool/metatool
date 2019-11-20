using Metatool.Service;
using static Metatool.Service.Key;

namespace Metatool.Tools.LibToolDemo
{
    [ToolConfig]
    public class Config
    {
        public string Option1 { get; set; }
        public int    Option2 { get; set; } = 5;
    }
    public class KeyboardConfig : CommandPackage
    {
        /// <summary>
        /// Global key
        /// </summary>
        public static Key GK = new Key(Space);

        /// <summary>
        /// Context key
        /// </summary>
        public static Key CK = new Key(Caps, Enter);

        /// <summary>
        /// Apps key
        /// </summary>
        public static Key AK = new Key(Apps, Tab);
    }
}


