using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Credentials;
using NuGet.Frameworks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Metatool.Script.NugetReference
{
    public class NugetPackage
    {
        private readonly IEnumerable<string>        _configFilePaths;
        private readonly IEnumerable<PackageSource> _packageSources;
        private readonly ExceptionDispatchInfo      _initializationException;
        private          ILogger      _logger;
        public           string                     GlobalPackageFolder { get; }
        public           string                     ToolPackageFolder { get; }
        public event Action<NuGetRestoreResult> RestoreResult;

        public NugetPackage(ILogger logger)
        {
            _logger = logger;
            try
            {
                ISettings settings;

                try
                {
                    settings = Settings.LoadDefaultSettings(
                        root: null,
                        configFileName: null,
                        machineWideSettings: new XPlatMachineWideSetting());
                }
                catch (NuGetConfigurationException ex)
                {
                    // create default settings using a non-existent config file
                    settings = new Settings(nameof(Script.ScriptRunner));
                }

                GlobalPackageFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
                _configFilePaths    = new List<string>(); //SettingsUtility.GetConfigFilePaths(settings);
                _packageSources     = SettingsUtility.GetEnabledSources(settings);
                var p = new PackageSource(Path.Combine(Context.AppDirectory, @".\pkg"),"metatool.pkg.source");
                _packageSources = _packageSources.Append(p);
                ToolPackageFolder = Path.Combine(Context.AppDirectory, @".\.pkg");
                    var p1 = new PackageSource(ToolPackageFolder, "metatool.pkg.used");
                _packageSources = _packageSources.Append(p1);
                DefaultCredentialServiceUtility.SetupDefaultCredentialService(NullLogger.Instance,
                    nonInteractive: false);

                var sourceProvider = new PackageSourceProvider(settings);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message + e.StackTrace);
                _initializationException = ExceptionDispatchInfo.Capture(e);
            }
        }

        internal RestoreParams CreateRestoreParams()
        {
            _initializationException?.Throw();

            var restoreParams = new RestoreParams();

            foreach (var packageSource in _packageSources)
            {
                restoreParams.Sources.Add(packageSource);
            }

            foreach (var configFile in _configFilePaths)
            {
                restoreParams.ConfigFilePaths.Add(configFile);
            }

            restoreParams.PackagesPath = ToolPackageFolder;

            return restoreParams;
        }

        internal void ParseLockFile(string lockFilePath, CancellationToken cancellationToken, NuGetFramework _targetFramework, string? _frameworkVersion, HashSet<LibraryRef> _libraries)
        {
            JObject obj;
            using (var reader = File.OpenText(lockFilePath))
            {
                obj = PackageUtils.LoadJson(reader);
            }

            var (compile, runtime, analyzers) = PackageUtils.ReadProjectLockJson(obj,
                ToolPackageFolder,
                _targetFramework.DotNetFrameworkName);

            TransformLockFileToDepsFile(obj, _targetFramework.DotNetFrameworkName, _libraries);

            cancellationToken.ThrowIfCancellationRequested();

            using (var writer = new JsonTextWriter(File.CreateText(lockFilePath)) {Formatting = Formatting.Indented})
            {
                obj.WriteTo(writer);
            }

            cancellationToken.ThrowIfCancellationRequested();
            RestoreResult?.Invoke(new NuGetRestoreResult(compile, runtime, analyzers));
        }

        private void TransformLockFileToDepsFile(JObject obj, string targetFramework, HashSet<LibraryRef> _libraries)
        {
            foreach (var p in obj.Properties().Where(p => p.Name != "targets" && p.Name != "libraries").ToArray())
            {
                p.Remove();
            }

            obj.AddFirst(new JProperty("runtimeTarget", new JObject(new JProperty("name", targetFramework))));

            var libraries = (JObject) obj["libraries"];

            foreach (var fx in ((JObject) obj["targets"]).Properties())
            {
                foreach (var p in fx.Value.Children<JProperty>().Where(IsRuntimeEmptyOrPlaceholder).ToArray())
                {
                    p.Remove();
                    libraries.Remove(p.Name);
                }

                foreach (var library in _libraries)
                {
                    if (library.Path != null)
                    {
                        ((JObject) fx.Value).Add(new JProperty(library.AssemblyName + "/0.0.0", new JObject(
                            new JProperty("type", "project"),
                            new JProperty("runtime", new JObject(
                                new JProperty(library.AssemblyName + ".dll", new JObject()))))));
                    }
                }
            }

            foreach (var p in libraries.Properties())
            {
                p.Value["serviceable"] = true;
                ((JObject) p.Value).Remove("files");
            }

            foreach (var library in _libraries)
            {
                if (library.Path != null)
                {
                    libraries.Add(new JProperty(library.AssemblyName + "/0.0.0", new JObject(
                        new JProperty("type", "project"),
                        new JProperty("serviceable", false),
                        new JProperty("sha512", ""))));
                }
            }

            bool IsRuntimeEmptyOrPlaceholder(JProperty p)
            {
                return !(p.Value["runtime"] is JObject runtime) ||
                       runtime.Properties().All(pp => PackageUtils.IsPlaceholder(pp.Name));
            }
        }
    }
}
