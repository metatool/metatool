using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.MetaPlugin;

namespace Metaseed.ScreenHint
{
    public class ScreenHintPlugin :PluginBase
    {
        private ScreenHint _screenHint;

        public override bool Init()
        {
            _screenHint = new ScreenHint();
            _screenHint.Hook();
            return base.Init();
        }
    }
}
