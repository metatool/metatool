using System;
using System.Collections.Generic;
using System.Text;
using Metaseed.MetaPlugin;

namespace Metaseed.ScreenHint
{
    public class ScreenHintPlugin :PluginBase
    {
        private Hint _hint;

        public override bool Init()
        {
            _hint = new Hint();
            _hint.Hook();
            return base.Init();
        }
    }
}
