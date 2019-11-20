using System;
using System.Collections.Generic;
using System.Text;
using Metatool.Service;
using Microsoft.Extensions.Configuration;

namespace Metaseed.Metatool
{
    public class MetatoolConfig
    {
        public MetatoolConfig(IConfig<Config> config, IKeyboard keyboard, IConfiguration configuration)
        {
            keyboard.AddAliases(config.CurrentValue.KeyAliases);
        }
    }
}
