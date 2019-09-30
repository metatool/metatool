using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Metatool.Metatool.Plugin;
using Metatool.Reactive;
using Metatool.Script;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metatool.Plugin
{
    public class PluginToken
    {
        public PluginLoader                Loader;
        public ObservableFileSystemWatcher Watcher;
        public List<IPlugin>               Tools = new List<IPlugin>();
    }

    public class PluginManager
    {
        const            string                 ScriptBin = "bin";
        private readonly ILogger<PluginManager> _logger;

        public PluginManager(ILogger<PluginManager> logger, IServiceProvider services)
        {
            _services = services;
            _logger   = logger;
        }

        private readonly IServiceProvider _services;

        readonly Dictionary<string, PluginToken> _plugins = new Dictionary<string, PluginToken>();

        public void InitPlugins()
        {
            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "tools");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var assemblyName = Path.GetFileName(dir);
                try
                {
                    var scriptPath = Path.Combine(dir, "main.csx");
                    var pluginDll = Path.Combine(dir, assemblyName + ".dll");
                    if (File.Exists(scriptPath))
                    {
                        pluginDll = Path.Combine(dir, ScriptBin, assemblyName + ".dll");
                        if (File.Exists(pluginDll))
                        {
                            var dllInfo    = new FileInfo(pluginDll);
                            var scriptInfo = new FileInfo(scriptPath);

                            if (scriptInfo.LastWriteTimeUtc > dllInfo.LastWriteTimeUtc)
                            {
                                BuildReload(scriptPath, assemblyName);
                            }
                            else
                            {
                                Load(scriptPath, assemblyName);
                            }
                        }
                        else
                        {
                            BuildReload(scriptPath, assemblyName);
                        }
                    }
                    else if (File.Exists(pluginDll))
                    {
                        Load(scriptPath, assemblyName, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{assemblyName}: Error while loading tool!");
                }
            }
        }

        private void Load(string scriptPath, string assemblyName, bool watch = true)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);
            var dllPath   = Path.Combine(pluginDir, ScriptBin, $"{assemblyName}.dll");

            ObservableFileSystemWatcher lastWatcher = null;

            static void move(string pluginDir1, string assemblyName1, ILogger logger1)
            {
                var rebuildPath = Path.Combine(pluginDir1, ScriptBin, AssemblyRebuildName(assemblyName1));
                var dllPath1    = Path.Combine(pluginDir1, ScriptBin, assemblyName1);

                if (File.Exists(rebuildPath + ".dll"))
                {
                    File.Move(rebuildPath + ".dll", dllPath1       + ".dll", true);
                    File.Move(rebuildPath + ".deps.json", dllPath1 + ".deps.json", true);
                    if (!Debugger.IsAttached)
                        File.Move(rebuildPath + ".pdb", dllPath1 + ".pdb", true);
                    logger1.LogInformation($"{assemblyName1}: replaced with new one");
                }
            }

            if (_plugins.ContainsKey(dllPath))
            {
                var plugin = _plugins[dllPath];
                if (plugin.Loader != null) // reload
                {
                    try
                    {
                        Unload(dllPath);
                        if (!plugin.Loader.IsAlive)
                        {
                            _logger.LogInformation($"{assemblyName}: unloaded!");
                            move(pluginDir, assemblyName, _logger);

                            Load(scriptPath, assemblyName);
                        }
                        else
                        {
                            _logger.LogError($"{assemblyName}: can NOT unload!");
                            if (watch) Watch(scriptPath, assemblyName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Reloading {assemblyName}: Can't unload!!!");
                    }

                    return;
                }

                lastWatcher = plugin.Watcher;
                _plugins.Remove(dllPath);
            }

            move(pluginDir, assemblyName, _logger);

            _logger.LogInformation($"{assemblyName}: Loading...");
            LoadDll(dllPath, lastWatcher);

            if (watch) Watch(scriptPath, assemblyName);
        }

        public void LoadDll(string dllPath, ObservableFileSystemWatcher lastWatcher = null)
        {
            Debug.Assert(Application.Current.Dispatcher != null, "Application.Current.Dispatcher != null");
            var access = Application.Current.Dispatcher.CheckAccess();
            if (!access)
            {
                Application.Current.Dispatcher.Invoke(() => LoadDll(dllPath, lastWatcher));
                return;
            }

            var assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            var loader       = CreatePluginLoader(dllPath);
            var token        = new PluginToken() {Loader = loader, Watcher = lastWatcher};
            _plugins.Add(dllPath, token);
            var pluginTypes = GetPluginTypes(loader);

            // var plugins = ServiceLocator.Current.GetServices<IMetaPlugin>(); only get newly added plugins
            var types = pluginTypes.ToList();
            if (types.Count == 0) _logger.LogWarning($"{assemblyName}: no tools defined");

            types.ForEach(t =>
            {
                var tool =
                    ActivatorUtilities.CreateInstance(_services, t) as IPlugin;
                tool?.OnLoaded();
                token.Tools.Add(tool);
            });
        }

        private void Unload(string dllPath)
        {
            var assemblyName = Path.GetFileName(dllPath);
            _logger.LogInformation($"{assemblyName}: start unloading...");
            //RemoveServices(_servicesCollection, plugin.Loader);
            var plugin = _plugins[dllPath];
            _plugins.Remove(dllPath);
            var tools = plugin.Tools;
            tools.ForEach(tool => tool.OnUnloading());
            tools.Clear();
            var loader = plugin.Loader;
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

        private void Watch(string scriptPath, string assemblyName)
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
                _logger.LogInformation($"Source file changed: {e.Name}");
                watcher.Stop().Dispose();
                _logger.LogInformation($"{assemblyName}: Stop watching source files");
                BuildReload(scriptPath, assemblyName);
            });
            watcher.subs.Add(sub);
            watcher.Start();
            _logger.LogInformation($"{assemblyName}: watching modification of *.csx");
        }

        private static string AssemblyRebuildName(string assemblyName) => assemblyName + "_build";

        public void BuildReload(string scriptPath, string assemblyName, bool watch = true)
        {
            _logger.LogInformation($"start to build assembly: {assemblyName}...");
            try
            {
                var scriptHost = new ScriptHost(_logger);
                var outputDir  = Path.Combine(Path.GetDirectoryName(scriptPath), ScriptBin);
                scriptHost.Build(scriptPath, outputDir, AssemblyRebuildName(assemblyName), OptimizationLevel.Debug);
                scriptHost.NotifyBuildResult += errors =>
                {
                    if (errors.Count > 0)
                    {
                        _logger.LogError($"Build Error({assemblyName}): " + string.Join(Environment.NewLine, errors));
                        if (watch) Watch(scriptPath, assemblyName);
                    }
                    else
                    {
                        _logger.LogInformation($"Assembly {assemblyName}: build successfully!");
                        Load(scriptPath, assemblyName, watch);
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Assembly {assemblyName}: build errors!");
                if (watch) Watch(scriptPath, assemblyName);
            }
        }

        private void RemoveServices(IServiceCollection services, PluginLoader loader)
        {
            foreach (var pluginType in loader
                .MainAssembly
                .GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract))
            {
                services.RemoveImplementation(pluginType);
            }

            // services.BuildServiceProvider();
        }

        private List<Type> GetPluginTypes(PluginLoader loader)

        {
            return loader.MainAssembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract)
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