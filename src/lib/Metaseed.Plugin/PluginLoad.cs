using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Metaseed.MetaPlugin;
using Metaseed.Metatool.Plugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Metaseed.Metaing
{
    public class PluginLoad
    {
        private static List<PluginLoader> GetPluginLoaders()
        {
            var loaders = new List<PluginLoader>();

            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "tools");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName   = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        pluginDll,
                        config => config.PreferSharedTypes = true);
                    loaders.Add(loader);
                }
            }

            return loaders;
        }

        private static void ConfigureServices(IServiceCollection services, List<PluginLoader> loaders)
        {
            foreach (var loader in loaders)
            {
                foreach (var pluginType in loader
                    .LoadDefaultAssembly()
                    .GetTypes()
                    .Where(t => typeof(IMetaPlugin).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    services.AddSingleton(typeof(IMetaPlugin), pluginType);
                }
            }
        }

        public static void Load(IServiceCollection services)
        {
            try
            {
                var loaders = GetPluginLoaders();
                ConfigureServices(services, loaders);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}