using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metaseed.Core;
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
        private static IServiceCollection _servicesCollection;

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

                        if (scriptInfo.LastWriteTimeUtc > dllInfo.LastWriteTimeUtc
                        ) // || scriptInfo.Length != dllInfo.Length)
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
                    var loader = CreatePluginLoader(pluginDll);
                    loaders.Add(loader);
                }
            }

            return loaders;
        }

        private static PluginLoader CreatePluginLoader(string pluginDll)
        {
            var loader = PluginLoader.CreateFromAssemblyFile(
                pluginDll,
                config => config.PreferSharedTypes = true);
            return loader;
        }

        private static void BuildScript(string path, string assemblyName, ILogger logger)
        {
            var scriptHost = new ScriptHost(logger);
            scriptHost.Build(path, assemblyName, OptimizationLevel.Debug);
            scriptHost.NotifyBuildResult += errors =>
            {
                if (errors.Count > 0)
                {
                    logger.LogError($"Build Error({assemblyName}): " + string.Join(Environment.NewLine, errors));
                }
                else
                {
                    //todo: unload then reload
                    // todo: file monitor
                    logger.LogInformation($"Assembly {assemblyName}: build successfully!");
                    var dllPath     = Path.Combine(Path.GetDirectoryName(path), $"{assemblyName}.dll");
                    var loader      = CreatePluginLoader(dllPath);
                    var pluginTypes = ConfigureServices(_servicesCollection, loader, false);
                    ServiceLocator.Current = _servicesCollection.BuildServiceProvider();
                    pluginTypes.ToList().ForEach(t =>
                    {
                        var plugin = ServiceLocator.Current.GetService(t) as IMetaPlugin;
                        plugin?.Init();
                    });
                }
            };
        }

        private static List<Type> ConfigureServices(IServiceCollection services, PluginLoader loader,
            bool isGeneral = true)
        {

            var r = new List<Type>();
            foreach (var pluginType in loader
                .LoadDefaultAssembly()
                .GetTypes()
                .Where(t => typeof(IMetaPlugin).IsAssignableFrom(t) && !t.IsAbstract))
            {
                if (isGeneral)
                    services.AddSingleton(typeof(IMetaPlugin), pluginType);
                else
                    services.AddSingleton(pluginType);

                r.Add( pluginType);
            }
            return r;
            
        }

        public static void Load(IServiceCollection services, ILogger logger)
        {
            _servicesCollection = services;
            try
            {
                var loaders = GetPluginLoaders(logger);
                foreach (var loader in loaders) ConfigureServices(services, loader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}