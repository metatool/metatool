using Metatool.Service;

namespace Metatool.Tools.Software
{
    [ToolConfig]
    public class Config
    {
        public string[] ConfigFolders { get; set; }
        public int    Option2 { get; set; } = 5;
    }
}
