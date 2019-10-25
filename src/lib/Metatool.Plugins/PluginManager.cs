using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Metatool.Metatool.Plugin;
using Metatool.Reactive;
using Metatool.Script;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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

        public PluginManager(ILogger<PluginManager> logger)
        {
            _logger = logger;
        }

        readonly Dictionary<string, PluginToken> _plugins = new Dictionary<string, PluginToken>();

        public void InitPlugins()
        {
            GetToolsDirectories().ForEach(InitPlugin);
        }

        public static IEnumerable<string> GetToolDirectories()
        {
            return GetToolsDirectories().SelectMany(Directory.GetDirectories).Where(dir =>
            {
                var assemblyName = Path.GetFileName(dir);
                var scriptPath   = Path.Combine(dir, "main.csx");
                var pluginDll    = Path.Combine(dir, assemblyName + ".dll");
                return File.Exists(scriptPath) || File.Exists(pluginDll);
            });
        }

        private static List<string> GetToolsDirectories()
        {
            static string NormalizePath(string path)
            {
                return Path.GetFullPath(new Uri(path).LocalPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
            }

            static List<string> AddToolPath(List<string> tools, string path)
            {
                path = NormalizePath(path);
                if (!Directory.Exists(path) || tools.Contains(path)) return tools;
                tools.Add(path);
                return tools;
            }

            var result = new List<string>()
            {
                // tools dir with metatool.dll
                Path.Combine(AppContext.BaseDirectory, "tools"),
                // tools dir with metatool.exe
                Path.Combine(Environment.CurrentDirectory, "tools")
            }.Aggregate(new List<string>(), AddToolPath);

            return result;
        }

        private void InitPlugin(string pluginsDir)
        {
            _logger.LogInformation($"Loading plugin tools from: {pluginsDir} ...");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var assemblyName = Path.GetFileName(dir);
                try
                {
                    var scriptPath = Path.Combine(dir, "main.csx");
                    var pluginDll  = Path.Combine(dir, assemblyName + ".dll");
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
                                Load(scriptPath, pluginDll, assemblyName);
                            }
                        }
                        else
                        {
                            BuildReload(scriptPath, assemblyName);
                        }
                    }
                    else if (File.Exists(pluginDll))
                    {
                        Load(scriptPath, pluginDll, assemblyName, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{assemblyName}: Error while loading tool!");
                }
            }
        }

        private void Load(string scriptPath, string dllPath, string assemblyName, bool watch = true)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);

            ObservableFileSystemWatcher lastWatcher = null;

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

                            Load(scriptPath, dllPath, assemblyName);
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

            LoadDll(dllPath, lastWatcher);

            if (watch) Watch(scriptPath, assemblyName);
        }


        public void LoadDll(string dllPath, ObservableFileSystemWatcher lastWatcher = null)
        {
            // Debug.Assert(Application.Current.Dispatcher != null, "Application.Current.Dispatcher != null");
            // var access = Application.Current.Dispatcher.CheckAccess();
            // if (!access)
            // {
            //     Application.Current.Dispatcher.Invoke(() => LoadDll(dllPath, lastWatcher));
            //     return;
            // }

            var assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            var loader       = CreatePluginLoader(dllPath);
            var token        = new PluginToken() { Loader = loader, Watcher = lastWatcher };
            _plugins.Add(dllPath, token);

            IServiceProviderDisposable provider = null;
            var allTypes   = loader.MainAssembly.GetTypes();
            var optionType = ToolConfig.GetOptionType(allTypes);
            if (optionType != null)
            {
                var services = new ServiceCollection();//Services.Get<IServiceCollection>();
                var id       = loader.MainAssembly.GetName().Name;
                var config   = Services.Get<IConfiguration>().GetSection(id);
                // call services.Configure<optionType>(Configuration.GetSection(id));
                var method = typeof(OptionsConfigurationServiceCollectionExtensions).GetMethod(
                    nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                    new[] { typeof(IServiceCollection), typeof(IConfiguration) }).MakeGenericMethod(optionType);
                method.Invoke(null, new object[] { services, config });

                provider = Services.AddServices(services);
            }
            (Assembly assembly, IEnumerable<Type> types) pluginTypes = (loader.MainAssembly, GetPluginTypes(allTypes));

            pluginTypes.assembly.EntryPoint?.Invoke(null, new object[] { });
            // var plugins = ServiceLocator.Current.GetServices<IMetaPlugin>(); only get newly added plugins
            var types = pluginTypes.types.ToList();
            if (types.Count == 0) _logger.LogWarning($"{assemblyName}: no tools defined");


            types.ForEach(t =>
            {
                var tool = provider==null?Services.Create<IPlugin>(t): provider.Create<IPlugin>(t);
                tool?.OnLoaded();
                token.Tools.Add(tool);
            });
            provider?.Dispose();
            _logger.LogInformation($"Tool Loaded: {assemblyName}");
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
                    config.PreferSharedTypes      = true;
                    config.IsUnloadable           = true;
                    config.SharedAssemblyPrefixes = new List<string>() {"Metatool.Plugin"};
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


        public void BuildReload(string scriptPath, string assemblyName, bool watch = true)
        {
            static void move(string pluginDir1, string assemblyName1, ILogger logger1)
            {
                var backupDir  = Path.Combine(pluginDir1, "backup");
                var backupPath = Path.Combine(backupDir, assemblyName1);
                var dllPath1   = Path.Combine(pluginDir1, assemblyName1);

                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                if (File.Exists(dllPath1 + ".dll"))
                {
                    File.Move(dllPath1 + ".dll", backupPath       + ".dll", true);
                    File.Move(dllPath1 + ".deps.json", backupPath + ".deps.json", true);
                    // if (!Debugger.IsAttached) for file is locked by vs
                    File.Move(dllPath1 + ".pdb", backupPath + ".pdb", true);
                    logger1.LogInformation($"{assemblyName1}: backup done");
                }
            }

            _logger.LogInformation($"start to build assembly: {assemblyName}...");
            try
            {
                var scriptHost = new ScriptHost(_logger);
                var outputDir  = Path.Combine(Path.GetDirectoryName(scriptPath), ScriptBin);
                var pluginDll  = Path.Combine(outputDir, assemblyName + ".dll");

                move(outputDir, assemblyName, _logger);
                scriptHost.Build(scriptPath, outputDir, assemblyName, OptimizationLevel.Debug);
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
                        Load(scriptPath, pluginDll, assemblyName, watch);
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

        private IEnumerable<Type> GetPluginTypes(IEnumerable<Type> types)
            => types
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
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