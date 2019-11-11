using System.IO;
using System.Linq;
using Metatool.Service;
using Microsoft.Extensions.Configuration;

namespace Metatool.Services
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
