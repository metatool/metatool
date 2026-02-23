using Metatool.MetaKeyboard;
using Metatool.Service;

namespace Metatool.Tools.KeyMouse
{
    public class KeyboardTool : ToolBase
    {
        private KeyboardMouseToolPackage _keyMouseTool;

        public override bool OnLoaded()
        {
            _keyMouseTool = Services.GetOrCreate<KeyboardMouseToolPackage>();
            return base.OnLoaded();
        }

        public KeyboardTool(ICommandManager commandManager, IConfig<PluginConfig> config)
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