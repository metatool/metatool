using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Metaseed.Script.NugetReference
{
    public class PackageUtils
    {
        internal static bool IsPlaceholder(string relativePath)
        {
            var  name         = Path.GetFileName(relativePath);
            var isPlacehlder = string.Equals(name, "_._", StringComparison.InvariantCulture);
            return isPlacehlder;
        }
        internal static JObject LoadJson(TextReader reader)
        {
            JObject obj;

            using (var jsonReader = new JsonTextReader(reader))
            {
                obj = JObject.Load(jsonReader);
            }

            return obj;
        }

        // internal List<LibraryRef> ParseReferences(string code)
        // {
        //     
        // }

        internal static (List<string> compile, List<string> runtime, List<string> analyzers) ReadProjectLockJson(JObject obj, string packagesDirectory, string framework)
        {
            var compile = new List<string>();
            var compileAssemblies = new HashSet<string>();
            var runtime = new List<string>();
            var runtimeAssemblies = new HashSet<string>();

            var targets = (JObject)obj["targets"];
            foreach (var target in targets)
            {
                if (target.Key == framework)
                {
                    foreach (var package in (JObject)target.Value)
                    {
                        var path = obj["libraries"][package.Key]["path"].Value<string>();
                        var packageRoot = Path.Combine(packagesDirectory, path);
                        if (ReadLockFileSection(packageRoot, package.Value, compile, compileAssemblies, nameof(compile)))
                        {
                            ReadLockFileSection(packageRoot, package.Value, runtime, runtimeAssemblies, nameof(runtime));
                        }
                    }

                    break;
                }
            }

            var analyzers = new List<string>();

            var libraries = (JObject)obj["libraries"];
            foreach (var library in libraries)
            {
                foreach (var item in (JArray)library.Value["files"])
                {
                    var file = item.Value<string>();
                    if (file.StartsWith("analyzers/dotnet/cs/", StringComparison.Ordinal))
                    {
                        var path = Path.Combine(packagesDirectory, library.Value["path"].Value<string>(), file);
                        analyzers.Add(path);
                    }
                }
            }

            return (compile, runtime, analyzers);
        }

        private static bool ReadLockFileSection(string packageRoot, JToken root, List<string> items, HashSet<string> names, string sectionName)
        {
            var section = (JObject)((JObject)root)[sectionName];
            if (section == null)
            {
                return false;
            }

            foreach (var item in section)
            {
                var relativePath = item.Key;
                // Ignore placeholder "_._" files.
                if (IsPlaceholder(relativePath))
                {
                    return false;
                }

                var name = Path.GetFileNameWithoutExtension(relativePath);
                // poor man's conflict resolution ;) take the first one
                if (names.Add(name))
                {
                    items.Add(Path.Combine(packageRoot, relativePath));
                }
            }

            return true;
        }

        internal static async Task<RestoreResult> RestoreAsync(RestoreParams restoreParameters, ILogger logger, CancellationToken cancellationToken = default)
        {
            var providerCache = new RestoreCommandProvidersCache();

            using var cacheContext = new SourceCacheContext {NoCache = false, IgnoreFailedSources = true};

            var providers = new List<IPreLoadedRestoreRequestProvider>();

            var dgSpec = new DependencyGraphSpec();
            dgSpec.AddRestore(restoreParameters.ProjectName);
            var projectSpec = new PackageSpec
            {
                Name             = restoreParameters.ProjectName,
                FilePath         = restoreParameters.ProjectName,
                RestoreMetadata  = CreateRestoreMetadata(restoreParameters),
                TargetFrameworks = { CreateTargetFramework(restoreParameters) }
            };
            dgSpec.AddProject(projectSpec);

            providers.Add(new DependencyGraphSpecRequestProvider(providerCache, dgSpec));

            var restoreContext = new RestoreArgs
            {
                CacheContext              = cacheContext,
                LockFileVersion           = LockFileFormat.Version,
                DisableParallel           = false,
                Log                       = logger,
                MachineWideSettings       = new XPlatMachineWideSetting(),
                PreLoadedRequestProviders = providers,
                AllowNoOp                 = true,
                HideWarningsAndErrors     = true
            };

            var restoreSummaries = await RestoreRunner.RunAsync(restoreContext, cancellationToken).ConfigureAwait(false);

            var result = new RestoreResult(
                success: restoreSummaries.All(x => x.Success),
                noOp: restoreSummaries.All(x => x.NoOpRestore),
                errors: restoreSummaries.SelectMany(x => x.Errors).Select(x => x.Message).ToImmutableArray());

            return result;
        }

        private static ProjectRestoreMetadata CreateRestoreMetadata(RestoreParams restoreParameters)
        {
            var metadata = new ProjectRestoreMetadata
            {
                ProjectUniqueName = restoreParameters.ProjectName,
                ProjectName = restoreParameters.ProjectName,
                ProjectStyle = ProjectStyle.PackageReference,
                ProjectPath = restoreParameters.ProjectName,
                OutputPath = restoreParameters.OutputPath,
                PackagesPath = restoreParameters.PackagesPath,
                ValidateRuntimeAssets = false,
                OriginalTargetFrameworks = { restoreParameters.TargetFramework.GetShortFolderName() }
            };

            foreach (var configPath in restoreParameters.ConfigFilePaths)
            {
                metadata.ConfigFilePaths.Add(configPath);
            }

            foreach (var source in restoreParameters.Sources)
            {
                metadata.Sources.Add(source);
            }

            return metadata;
        }

        private static TargetFrameworkInformation CreateTargetFramework(RestoreParams restoreParameters)
        {
            var targetFramework = new TargetFrameworkInformation
            {
                FrameworkName = restoreParameters.TargetFramework
            };

            if (restoreParameters.TargetFramework.Framework == ".NETCoreApp")
            {
                // targetFramework.Dependencies.Add(new LibraryDependency(
                //     libraryRange: new LibraryRange("Microsoft.NETCore.App",
                //         new VersionRange(restoreParameters.FrameworkVersion != null
                //             ? new NuGetVersion(restoreParameters.FrameworkVersion)
                //             : new NuGetVersion(restoreParameters.TargetFramework.Version)), LibraryDependencyTarget.Package),
                //     type: LibraryDependencyType.Platform,
                //     includeType: LibraryIncludeFlags.All,
                //     suppressParent: LibraryIncludeFlags.All,
                //     noWarn: Array.Empty<NuGetLogCode>(),
                //     autoReferenced: true,
                //     generatePathProperty:true));
            }

            if (restoreParameters.Libraries != null)
            {
                foreach (var package in restoreParameters.Libraries)
                {
                    AddPackageToFramework(targetFramework, package);
                }
            }

            return targetFramework;
        }

        private static void AddPackageToFramework(TargetFrameworkInformation targetFramework, LibraryRef library)
        {
            if (library.Path != null)
            {
                return;
            }

            targetFramework.Dependencies.Add(new LibraryDependency
            {
                LibraryRange = new LibraryRange(library.Id, library.VersionRange, LibraryDependencyTarget.Package)
            });
        }

    }

}
