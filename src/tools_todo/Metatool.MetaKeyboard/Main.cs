using Metatool.MetaKeyboard;
using Metatool.Service;

namespace Metatool.Tools.MetaKeyboard
{
    public class KeyboardTool : ToolBase
    {
        public override bool OnLoaded()
        {
            var keyboard61   = Services.GetOrCreate<Keyboard61>();
            var metaEditor= Services.GetOrCreate<MetaEditor>();
            var mouse        = Services.GetOrCreate<KeyboardMouse>();
            var fileExplorer = Services.GetOrCreate<FileExplorer>();
            var software     = Services.GetOrCreate<Software>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IConfig<Config> config)
        {
            //var a = config.CurrentValue.Test.Values;
            RegisterCommands();
        }
    }
}