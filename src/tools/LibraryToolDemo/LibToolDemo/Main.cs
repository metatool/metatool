using Metatool.Command;
using Metatool.Input;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;
using static Metatool.Input.Key;

namespace Metatool.Tools
{
    public class ToolDemo : PluginBase
    {
        IRemove token;

        public ToolDemo(ICommandManager commandManager, IKeyboard keyboard)
        {
            token = commandManager.Add(keyboard.Down(Caps + A),
                e => { Logger.LogInformation($"{nameof(ToolDemo)}: Caps+A triggered!!!!!!!"); });
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