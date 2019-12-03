using Metatool.Service;
using Microsoft.Extensions.Logging;
using static Metatool.Service.Key;
using static Metatool.Tools.LibToolDemo.KeyboardConfig;
namespace Metatool.Tools.LibToolDemo
{
    public class ToolDemo : ToolBase
    {
        public ICommandToken<IKeyEventArgs> CommandA;
        public IKeyCommand CommandB;

        public ToolDemo(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config)
        {
            CommandA = commandManager.Add(keyboard.OnDown(AK + A),
                e => { Logger.LogInformation($"{nameof(ToolDemo)}: Caps+A triggered!!!!!!!"); });
            CommandB = (Caps + B).OnDown(e => Logger.LogWarning("Caps+B pressed!!!"));

            Logger.LogInformation(config.CurrentValue.Option1);
            Logger.LogInformation(config.CurrentValue.Option2.ToString());
            RegisterCommands();
        }

        public override bool OnLoaded()
        {
            Logger.LogInformation($"all other tools are already created here");
            return base.OnLoaded();
        }

        public override void OnUnloading()
        {
            CommandA.Remove();
            base.OnUnloading();
        }
    }
}
