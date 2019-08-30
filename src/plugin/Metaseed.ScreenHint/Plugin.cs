using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.Metaing;
using Metaseed.MetaPlugin;
using Microsoft.Extensions.Logging;

namespace Metaseed.ScreenHint
{
    public class ScreenHintPlugin :PluginBase
    {
        private ScreenHint _screenHint;

        public ScreenHintPlugin(IMy iMy, ILogger<ScreenHintPlugin> logger)
        {
            iMy.SomeMethod();
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
