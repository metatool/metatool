using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Metaseed.MetaPlugin;
using Metaseed.Metatool.Plugin;
using Metaseed.Reactive;
using Metaseed.Script;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaseed.Plugin
{
    public class PluginToken
    {
        public PluginLoader                Loader;
        public ObservableFileSystemWatcher Watcher;
    }

    public class PluginManager
    {
        public static PluginManager Inst = new PluginManager();

        private PluginManager()
        {
        }

        private IServiceCollection _servicesCollection;

        readonly Dictionary<string, PluginToken> _plugins = new Dictionary<string, PluginToken>();

        public void InitPlugins(IServiceCollection services, ILogger logger)
        {
            _servicesCollection = services;
            try
            {
                initPlugins(logger);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void initPlugins(ILogger logger)
        {
            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "tools");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var assemblyName = Path.GetFileName(dir);
                var pluginDll    = Path.Combine(dir, assemblyName + ".dll");
                var scriptPath = Path.Combine(dir, "main.csx");

                if (File.Exists(scriptPath))
                {
                    if (File.Exists(pluginDll))
                    {
                        var dllInfo    = new FileInfo(pluginDll);
                        var scriptInfo = new FileInfo(scriptPath);

                        if (scriptInfo.LastWriteTimeUtc > dllInfo.LastWriteTimeUtc)
                        {
                            BuildReload(scriptPath, assemblyName, logger);
                        }
                        else
                        {
                            Load(scriptPath, assemblyName, logger);
                        }
                    }
                    else
                    {
                        BuildReload(scriptPath, assemblyName, logger);
                    }

                }
                else if (File.Exists(pluginDll))
                {
                    Load(scriptPath, assemblyName, logger, false);
                }
            }
        }

        private void Load(string scriptPath, string assemblyName, ILogger logger, bool watch = true)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);
            var dllPath   = Path.Combine(pluginDir, $"{assemblyName}.dll");

            ObservableFileSystemWatcher lastWatcher = null;
            if (_plugins.ContainsKey(dllPath))
            {
                static void move(string pluginDir1, string assemblyName1, ILogger logger1)
                {
                    var rebuildPath = Path.Combine(pluginDir1, AssemblyRebuildName(assemblyName1));
                    var dllPath1    = Path.Combine(pluginDir1, assemblyName1);

                    if (File.Exists(rebuildPath + ".dll"))
                    {
                        File.Move(rebuildPath + ".dll", dllPath1       + ".dll", true);
                        File.Move(rebuildPath + ".deps.json", dllPath1 + ".deps.json", true);
                        if (!Debugger.IsAttached)
                            File.Move(rebuildPath + ".pdb", dllPath1       + ".pdb", true);
                    }
                    logger1.LogInformation($"{assemblyName1}: plugin replaced with new one");
                }

                var plugin = _plugins[dllPath];
                if (plugin.Loader != null) // reload
                {
                    try
                    {
                        Unload(dllPath, logger);
                        if (!plugin.Loader.IsAlive)
                        {
                            logger.LogInformation($"{assemblyName}: unloaded!");
                            move(pluginDir, assemblyName,logger);
                            
                            Load(scriptPath, assemblyName, logger);
                        }
                        else
                        {
                            logger.LogError($"{assemblyName}: can NOT unload!");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Reloading {assemblyName}: Can't unload!!!");
                    }

                    return;
                }

                lastWatcher = plugin.Watcher;
                _plugins.Remove(dllPath);
                move(pluginDir, assemblyName,logger);
            }

            logger.LogInformation($"{assemblyName}: Loading Plugin.");
            var loader = CreatePluginLoader(dllPath);
            _plugins.Add(dllPath, new PluginToken() {Loader = loader, Watcher = lastWatcher});
            var pluginTypes = GetPluginTypes(loader);
            // var plugins = ServiceLocator.Current.GetServices<IMetaPlugin>(); only get newly added plugins
            pluginTypes.ToList().ForEach(t =>
            {
                var plugin =
                    ActivatorUtilities.CreateInstance(_servicesCollection.BuildServiceProvider(), t) as IMetaPlugin;
                plugin?.Init();
            });

            if(watch)Watch(scriptPath, assemblyName, logger);
        }

        private void Unload(string dllPath,ILogger logger)
        {
            var assemblyName = Path.GetFileName(dllPath);
            logger.LogInformation($"{assemblyName}: start unloading...");
            //RemoveServices(_servicesCollection, plugin.Loader);
            var plugin = _plugins[dllPath];
            _plugins.Remove(dllPath);
            var loader = plugin.Loader;
            plugin.Watcher?.Stop().Dispose();
            loader?.Dispose();
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
                _plugins[dllPath].Watcher = watcher;
            }
            else
            {
                _plugins.Add(dllPath, new PluginToken() {Watcher = watcher});
            }

            var sub = watcher.Changed.Throttle(TimeSpan.FromSeconds(0.5)).Subscribe(e =>
            {
                logger.LogInformation($"Source file changed: {e.Name}");
                watcher.Stop().Dispose();
                BuildReload(scriptPath, assemblyName, logger);
            });
            watcher.subs.Add(sub);
            watcher.Start();
            logger.LogInformation($"{assemblyName}: watching modification of *.csx");
        }

        private static string AssemblyRebuildName(string assemblyName) => assemblyName + "_build";

        private void BuildReload(string scriptPath, string assemblyName, ILogger logger)
        {
            logger.LogInformation($"start to build assembly: {assemblyName}...");
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

            // services.BuildServiceProvider();
        }

        private List<Type> GetPluginTypes(PluginLoader loader)

        {
            return loader.MainAssembly.GetTypes()
                .Where(t => typeof(IMetaPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
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