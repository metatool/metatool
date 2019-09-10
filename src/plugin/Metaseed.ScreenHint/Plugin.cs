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

    internal sealed class Program
    {
        public class AA: PluginBase
        {
            private readonly IMy _iMy;
            private readonly ILogger<ScreenHintPlugin> _logger;

            public AA(IMy iMy, ILogger<ScreenHintPlugin> logger)
            {
                _iMy = iMy;
                _logger = logger;
            }
            public override bool Init()
            {
                _logger.LogError("dsdfsdfsdfd++++++++");
                return base.Init();
            }


        }
    }
}
