using Metatool.Service;

namespace Metatool.Tools.WinShell
{
    public class WinShellTool : ToolBase
    {
        private FileExplorer _fileExplorer;
        private TaskView _taskView;

        public override bool OnLoaded()
        {
            _fileExplorer = Services.GetOrCreate<FileExplorer>();
            _taskView = Services.GetOrCreate<TaskView>();
            return base.OnLoaded();
        }

        public WinShellTool(ICommandManager commandManager, IConfig<Config> config)
        {
            RegisterCommands();
        }
    }
}
