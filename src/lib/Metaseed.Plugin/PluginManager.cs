using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Metaseed.Core;
using Metaseed.MetaPlugin;
using Metaseed.Metatool.Plugin;
using Metaseed.Reactive;
using Metaseed.Script;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaseed.Plugin
{
    public class PluginManager
    {
        public static PluginManager Inst = new PluginManager();

        private PluginManager()
        {
        }

        private IServiceCollection _servicesCollection;

        readonly Dictionary<string, (PluginLoader loader, ObservableFileSystemWatcher watcher)> _plugins =
            new Dictionary<string, (PluginLoader, ObservableFileSystemWatcher)>();

        public void InitPlugins(IServiceCollection services, ILogger logger)
        {
            _servicesCollection = services;
            try
            {
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
                                BuildReload(pluginScript, dirName, logger);
                            }
                            else
                            {
                                Load(pluginScript, dirName, logger);
                            }
                        }
                        else
                        {
                            BuildReload(pluginScript, dirName, logger);
                        }

                        Watch(pluginScript, dirName, logger);
                    }
                    else if (File.Exists(pluginDll))
                    {
                        Load(pluginScript, dirName, logger);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Load(string scriptPath, string assemblyName, ILogger logger)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);
            var dllPath   = Path.Combine(pluginDir, $"{assemblyName}.dll");

            ObservableFileSystemWatcher lastWatcher = null;
            if (_plugins.ContainsKey(dllPath))
            {
                static void move(string pluginDir1, string assemblyName1)
                {
                    var rebuildPath = Path.Combine(pluginDir1, AssemblyRebuildName(assemblyName1));
                    var dllPath1    = Path.Combine(pluginDir1, assemblyName1);
                    if (File.Exists(rebuildPath + ".dll"))
                    {
                        File.Move(rebuildPath + ".dll", dllPath1       + ".dll", true);
                        File.Move(rebuildPath + ".deps.json", dllPath1 + ".deps.json", true);
                        File.Move(rebuildPath + ".pdb", dllPath1       + ".pdb", true);
                    }
                }
                var plugin = _plugins[dllPath];
                if (plugin.loader != null) // reload
                {
                    try
                    {
                        RemoveServices(_servicesCollection, plugin.loader);
                        ServiceLocator.Current = _servicesCollection.BuildServiceProvider();
                            plugin.watcher?.Stop().Dispose();
                            _plugins.Remove(dllPath);
                        plugin.loader.Dispose();
                        if (!plugin.loader.IsAlive)
                        {
                            move(pluginDir, assemblyName);
                            logger.LogInformation($"{assemblyName}: plugin unloaded!");
                            Load(scriptPath, assemblyName, logger);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Reloading {assemblyName}: Can't unload!!!");
                    }

                    return;
                }
                else
                {
                    lastWatcher = plugin.watcher;
                    _plugins.Remove(dllPath);
                    move(pluginDir, assemblyName);
                }
            }

            logger.LogInformation($"{assemblyName}: Loading Plugin.");
            var loader = CreatePluginLoader(dllPath);
            _plugins.Add(dllPath, (loader, lastWatcher));

            var pluginTypes = ConfigureServices(_servicesCollection, loader, false);
            ServiceLocator.Current = _servicesCollection.BuildServiceProvider();

            // var plugins = ServiceLocator.Current.GetServices<IMetaPlugin>(); only get newly added plugins
            pluginTypes.ToList().ForEach(t =>
            {
                var plugin = ServiceLocator.Current.GetService(t) as IMetaPlugin;
                plugin?.Init();
            });
        }

        private PluginLoader CreatePluginLoader(string pluginDll)
        {
            var loader = PluginLoader.CreateFromAssemblyFile(
                pluginDll,
                config =>
                {
                    config.PreferSharedTypes = true;
                    config.IsUnloadable      = true;
                });
            return loader;
        }

        private void Watch(string scriptPath, string assemblyName, ILogger logger)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);
            var dllPath   = Path.Combine(pluginDir, $"{assemblyName}.dll");

            var watcher = new ObservableFileSystemWatcher(c =>
            {
                c.Path         = pluginDir;
                c.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                c.Filter       = "*.csx";
            });

            if (_plugins.ContainsKey(dllPath))
            {
                var plugin = _plugins[dllPath];
                _plugins[dllPath] = (plugin.loader, watcher);
            }
            else
            {
                _plugins.Add(dllPath, (null, watcher));
            }

            var sub= watcher.Changed.Throttle(TimeSpan.FromSeconds(.5)).Subscribe(e =>
            {
                BuildReload(scriptPath, assemblyName, logger);
            });
            watcher.subs.Add(sub);
            watcher.Start();
            logger.LogInformation($"{assemblyName}: watching modification of *.csx");
        }

        private static string AssemblyRebuildName(string assemblyName) => assemblyName + "_build";

        private void BuildReload(string scriptPath, string assemblyName, ILogger logger)
        {
            var scriptHost = new ScriptHost(logger);
            scriptHost.Build(scriptPath, AssemblyRebuildName(assemblyName), OptimizationLevel.Debug);
            scriptHost.NotifyBuildResult += errors =>
            {
                if (errors.Count > 0)
                {
                    logger.LogError($"Build Error({assemblyName}): " + string.Join(Environment.NewLine, errors));
                }
                else
                {
                    logger.LogInformation($"Assembly {assemblyName}: build successfully!");
                    Load(scriptPath, assemblyName, logger);
                }
            };
        }

        private void RemoveServices(IServiceCollection services, PluginLoader loader)
        {
            foreach (var pluginType in loader
                .MainAssembly
                .GetTypes()
                .Where(t => typeof(IMetaPlugin).IsAssignableFrom(t) && !t.IsAbstract))
            {
                services.RemoveImplementation(pluginType);
            }
        }

        private List<Type> ConfigureServices(IServiceCollection services, PluginLoader loader,
            bool isGeneral = true)
        {
            var r = new List<Type>();
            foreach (var pluginType in loader
                .MainAssembly
                .GetTypes()
                .Where(t => typeof(IMetaPlugin).IsAssignableFrom(t) && !t.IsAbstract))
            {
                services.RemoveImplementation(pluginType);
                if (isGeneral)
                    services.AddSingleton(typeof(IMetaPlugin), pluginType);
                else
                    services.AddSingleton(pluginType);

                r.Add(pluginType);
            }

            return r;
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
            if (serviceDescriptor != null) services.Remove(serviceDescriptor);

            return services;
        }

        public static IServiceCollection RemoveImplementation<T>(this IServiceCollection services)
        {
            return services.RemoveImplementation(typeof(T));
        }

        public static IServiceCollection RemoveImplementation(this IServiceCollection services, Type implementationType)
        {
            var serviceDescriptor =
                services.FirstOrDefault(descriptor => descriptor.ImplementationType == implementationType);
            if (serviceDescriptor != null) services.Remove(serviceDescriptor);

            return services;
        }
    }
}