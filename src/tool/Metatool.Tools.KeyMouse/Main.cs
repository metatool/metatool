using KeyMouse;
using Metatool.Service;

namespace Metatool.Tools.KeyMouse
{
    public class KeyboardTool : ToolBase
    {
        private KeyMouseToolPackage _keyMouseTool;

        public override bool OnLoaded()
        {
            _keyMouseTool = Services.GetOrCreate<KeyMouseToolPackage>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IConfig<Config> config)
        {
            //var a = config.CurrentValue.Test.Values;
            RegisterCommands();
        }

        public override void OnUnloading()
        {
            _keyMouseTool.OnUnloading();
            base.OnUnloading();
        }
    }
}