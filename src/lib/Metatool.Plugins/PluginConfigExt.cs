using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Metatool.Plugin;
using Metatool.Plugin.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;

namespace Metatool.Plugins
{
    public static class PluginConfigExt
    {
        public static IConfigurationBuilder AddPluginsConfig(this IConfigurationBuilder builder)
        {
            var toolsDirs = PluginManager.GetToolDirectories();

            toolsDirs.ToList().ForEach(d =>
            {
                builder.AddJsonFile(Path.Combine(d, "config.json"), optional: true,
                    reloadOnChange: true);

            }) ;
            return builder;
        }
    }
}
