using System;
using System.Collections.Generic;
using System.Text;
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

        public override bool Init()
        {
            _screenHint = new ScreenHint();
            _screenHint.Hook();
            return base.Init();
        }
    }
}
