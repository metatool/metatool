using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.MetaPlugin;
using Microsoft.Extensions.Logging;

namespace Metaseed.ScreenHint
{
    public class ScreenHintPlugin : PluginBase
    {
        private ScreenHint _screenHint;

        public ScreenHintPlugin(ILogger<ScreenHintPlugin> logger)
        {
            logger.LogInformation("Hello!!!!!!!!!!");
        }

        public override bool Init()
        {
            _screenHint = new ScreenHint();
            _screenHint.Hook();
            return base.Init();
        }
    }
}