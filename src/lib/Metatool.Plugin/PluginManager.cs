using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Metatool.Plugin;
using Metatool.NugetPackage;
using Metatool.Reactive;
using Metatool.Script;
using Metatool.Service;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NuGet.Configuration;
using NuGet.Versioning;

namespace Metatool.Plugin
{
    public class PluginManager
    {
        const string ScriptBin = "bin";
        private readonly ILogger<PluginManager> _logger;

        public PluginManager(ILogger<PluginManager> logger)
        {
            _logger = logger;
        }

        readonly Dictionary<string, PluginToken> _plugins = new Dictionary<string, PluginToken>();

        public static IEnumerable<string> GetToolDirectories()
        {
            return GetToolsDirectories().SelectMany(Directory.GetDirectories).Where(dir =>
            {
                var assemblyName = Path.GetFileName(dir);
                var scriptPath = Path.Combine(dir, "main.csx");
                var pluginDll = Path.Combine(dir, assemblyName + ".dll");
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
                Path.Combine(Context.AppDirectory, "tools"),
                Path.Combine(Context.BaseDirectory, "tools"),
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
                InitPluginTool(dir, assemblyName);
            }
        }

        private void InitPluginTool(string dir, string assemblyName)
        {
            try
            {
                var scriptPath = Path.Combine(dir, "main.csx");
                var pluginDll = Path.Combine(dir, assemblyName + ".dll");
                if (File.Exists(scriptPath))
                {
                    pluginDll = Path.Combine(dir, ScriptBin, assemblyName + ".dll");
                    if (File.Exists(pluginDll))
                    {
                        var dllInfo = new FileInfo(pluginDll);
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
                else
                {
                    static string GetPath(string toolDir)
                    {
                        var verRegex = new Regex("(\\d+\\.)?(\\d+\\.)?(\\*|\\d+)",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        var latestVersionFolder = Directory.EnumerateDirectories(toolDir)
                            .Where(d => verRegex.IsMatch(d))
                            .OrderBy(k => k).LastOrDefault();
                        if (latestVersionFolder != null)
                        {
                            var toolsFolder = Path.Combine(latestVersionFolder, "tools");
                            if (Directory.Exists(toolsFolder))
                                return toolsFolder;
                            var libFolder = Path.Combine(latestVersionFolder, "lib");
                            if (Directory.Exists(toolsFolder))
                            {
                                var runtime = new Regex("netcoreapp[\\d.]+",
                                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                var latestRuntimeFolder = Directory.EnumerateDirectories(libFolder)
                                    .Where(d => runtime.IsMatch(d))
                                    .OrderBy(k => k).LastOrDefault();
                                if (Directory.Exists(latestRuntimeFolder))
                                    return toolsFolder;
                            }
                        }

                        return null;
                    }

                    var path = GetPath(dir);
                    if (path == null) return;
                    InitPluginTool(path, assemblyName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{assemblyName}: Error while loading tool!");
            }
        }

        private void Load(string scriptPath, string dllPath, string assemblyName, bool watch = true)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);

            ObservableFileSystemWatcher lastWatcher = null;

            if (_plugins.ContainsKey(assemblyName))
            {
                var plugin = _plugins[assemblyName];
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
                _plugins.Remove(assemblyName);
            }

            LoadDll(dllPath, lastWatcher);

            if (watch) Watch(scriptPath, assemblyName);
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }

            Directory.Delete(source, true);
        }

        public void InitPlugins()
        {
            var toolDirs = GetToolsDirectories(); //.Where(d => !toolIds.Any(d.EndsWith)).ToList();
            toolDirs.ForEach(InitPlugin);
            Task.Run(UpdateTools);
        }

        private async Task UpdateTools()
        {
            var toolIds = Services.Get<IConfiguration>().GetSection("Tools").GetChildren()
                .Where(t => t.GetValue<bool>("Update") == true).Select(t => t.Key);
            var nugetManager = new NugetManager(_logger);
            var nugetFinder = new PackageFinder();

            foreach (var toolId in toolIds)
            {
                var sources = nugetManager.SourceRepositories;
                var r = await nugetFinder.GetLatestPackage(toolId, sources, false, false, false);
                if (r.metadata == null)
                {
                    _logger.LogDebug($"No Package({toolId}) found in any source!");
                    continue;
                }

                _plugins.TryGetValue(toolId, out var token);
                var version = token?.Version;
                if (version != null && r.metadata.Identity.Version.Version <= version)
                {
                    _logger.LogDebug($"Package({toolId}) is already the latest version.");
                    continue;
                }

                _logger.LogInformation($"{toolId}: new version available, updating...");

                nugetManager.Id = toolId + "_Restore";
                nugetManager.RestorePath = Path.Combine(Context.DefaultToolsDirectory, toolId);

                var re = await nugetManager.RefreshPackagesAsync(
                    new[] { new LibraryRef(toolId, VersionRange.All) }, CancellationToken.None,
                    new List<PackageSource>() { r.source });
                if (re.Success)
                {
                    var toolDir = Path.Combine(Context.DefaultToolsDirectory, toolId);
                    MoveDirectory(Path.Combine(Context.PackageDirectory, toolId), toolDir);
                    _logger.LogInformation($"{toolId}: Restore Success");
                    InitPluginTool(toolDir, toolId);
                }
                else
                {
                    foreach (var error in re.Errors)
                    {
                        _logger.LogError(error);
                    }
                }

                ;
            }
        }

        public void LoadDll(string dllPath, ObservableFileSystemWatcher lastWatcher = null)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            var loader = CreatePluginLoader(dllPath);
            var token = new PluginToken() { Loader = loader, Watcher = lastWatcher };
            _plugins.Add(assemblyName, token);

            IDisposableServiceProvider provider = null;
            var allTypes = loader.MainAssembly.GetTypes();
            var optionType = ToolConfigAttribute.GetConfigType(allTypes);
            if (optionType != null)
            {
                var services = new ServiceCollection();

                var id = loader.MainAssembly.GetName().Name;
                var configRoot = Services.Get<IConfiguration>();
                var config = configRoot.GetSection("Tools").GetSection(id);
                services.Configure<MetatoolConfig>(configRoot);

                // call services.Configure<optionType>(config);
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
                var tool = provider == null ? Services.Create<IPlugin>(t) : provider.Create<IPlugin>(t);
                tool?.OnLoaded();
                token.Tools.Add(tool);
            });
            provider?.Dispose();
            _logger.LogInformation($"Tool Loaded: {assemblyName} - Version: {token.Version}");
        }

        private void Unload(string dllPath)
        {
            var assemblyName = Path.GetFileName(dllPath);
            _logger.LogInformation($"{assemblyName}: start unloading...");
            //RemoveServices(_servicesCollection, plugin.Loader);
            var plugin = _plugins[assemblyName];
            _plugins.Remove(assemblyName);
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
                    config.IsUnloadable = true;
                    config.SharedAssemblyPrefixes = new List<string>() { "Metatool.Plugin" };
                });
            return loader;
        }

        private void Watch(string scriptPath, string assemblyName)
        {
            var pluginDir = Path.GetDirectoryName(scriptPath);
            var dllPath = Path.Combine(pluginDir, $"{assemblyName}.dll");

            var watcher = new ObservableFileSystemWatcher(c =>
            {
                c.Path = pluginDir;
                c.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                c.Filter = "*.csx";
            });

            if (_plugins.ContainsKey(assemblyName))
            {
                _plugins[assemblyName].Watcher = watcher;
            }
            else
            {
                _plugins.Add(assemblyName, new PluginToken() { Watcher = watcher });
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
            static void backup(string pluginDir1, string assemblyName1, ILogger logger1)
            {
                var backupDir = Path.Combine(pluginDir1, "backup");
                var backupPath = Path.Combine(backupDir, assemblyName1);
                var dllPath1 = Path.Combine(pluginDir1, assemblyName1);

                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                if (File.Exists(dllPath1 + ".dll"))
                {
                    File.Move(dllPath1 + ".dll", backupPath + ".dll", true);
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
                var outputDir = Path.Combine(Path.GetDirectoryName(scriptPath), ScriptBin);
                var dll = Path.Combine(outputDir, assemblyName + ".dll");

                backup(outputDir, assemblyName, _logger);
                Task.Run(()=>scriptHost.Build(scriptPath, outputDir, assemblyName, OptimizationLevel.Debug));
                scriptHost.NotifyBuildResult += errors =>
                {
                    if (errors.Count > 0)
                    {
                        if (watch) Watch(scriptPath, assemblyName);
                    }
                    else
                    {
                        Load(scriptPath, dll, assemblyName, watch);
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

}