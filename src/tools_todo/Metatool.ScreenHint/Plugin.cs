using Metatool.Plugin;
using Microsoft.Extensions.Logging;

namespace Metatool.ScreenHint
{
    public class ScreenHintPlugin : PluginBase
    {
        private ScreenHint _screenHint;

        public ScreenHintPlugin(ILogger<ScreenHintPlugin> logger) : base(logger)
        {
        }

        public override bool OnLoaded()
        {
            _screenHint = new ScreenHint();
            return base.OnLoaded();
        }
    }
}
