using Metatool.Command;
using Metatool.Input;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;
using static Metatool.Input.Key;

namespace Metatool.Tools.LibToolDemo
{
    public class ToolDemo : ToolBase
    {
        IRemove token;

        public ToolDemo(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config)
        {
            token = commandManager.Add(keyboard.Down(Caps + A),
                e => { Logger.LogInformation($"{nameof(ToolDemo)}: Caps+A triggered!!!!!!!"); });
            Logger.LogInformation(config.CurrentValue.Option1);
            Logger.LogInformation(config.CurrentValue.Option2.ToString());
        }

        public override bool OnLoaded()
        {
            Logger.LogInformation($"all other tools are already created here");
            return base.OnLoaded();
        }

        public override void OnUnloading()
        {
            token.Remove();
            base.OnUnloading();
        }
    }
}