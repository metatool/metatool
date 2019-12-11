using Metatool.Service;

namespace Metatool.Tools.LibToolDemo
{
    [ToolConfig]
    public class Config
    {
        public string SoftwareFolder { get; set; }
        public int    Option2 { get; set; } = 5;
    }
}
