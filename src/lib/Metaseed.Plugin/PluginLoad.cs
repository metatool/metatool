using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metaseed.MetaPlugin;
using Metaseed.Metatool.Plugin;
using Metaseed.Script;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaseed.Plugin
{
    public class PluginLoad
    {
        private static List<PluginLoader> GetPluginLoaders(ILogger logger)
        {
            var loaders = new List<PluginLoader>();

            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "tools");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName      = Path.GetFileName(dir);
                var pluginDll    = Path.Combine(dir, dirName + ".dll");
                var pluginScript = Path.Combine(dir, "main.csx");

                if (File.Exists(pluginScript))
                {
                    if (File.Exists(pluginDll))
                    {
                        var dllInfo    = new FileInfo(pluginDll);
                        var scriptInfo = new FileInfo(pluginScript);

                        if (scriptInfo.LastWriteTimeUtc > dllInfo.LastWriteTimeUtc)
                        {
                            // rebuild
                            // error continue
                            BuildScript(pluginScript, dirName, logger);
                        }
                    }
                    else
                    {
                        BuildScript(pluginScript, dirName, logger);
                    }
                }

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

        private static void BuildScript(string path, string assemblyName, ILogger logger)
        {
            var scriptHost = new ScriptHost(logger);
            scriptHost.Build(path, assemblyName, OptimizationLevel.Debug);
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

        public static void Load(IServiceCollection services, ILogger logger)
        {
            try
            {
                var loaders = GetPluginLoaders(logger);
                ConfigureServices(services, loaders);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}