using Metatool.Service;

namespace Metatool.Tools.WinShell
{
    public class WinShellTool : ToolBase
    {
        private FileExplorer _fileExplorer;

        public override bool OnLoaded()
        {
            _fileExplorer = Services.GetOrCreate<FileExplorer>();
            return base.OnLoaded();
        }

        public WinShellTool(ICommandManager commandManager, IConfig<Config> config)
        {
            RegisterCommands();
        }
    }
}
