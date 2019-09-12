using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metaseed.Core.ViewModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Frameworks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Metaseed.Script.NugetReference
{
    public class PackageViewModel : NotificationObject
    {
        private bool _isRestoring;
        private bool _restoreFailed;
        private readonly ILogger _logger;
        private IReadOnlyList<string> _restoreErrors;
        private readonly SemaphoreSlim _restoreLock;
        private readonly NugetPackage _nugetPackage;
        private readonly HashSet<LibraryRef> _libraries;
        private NuGetFramework _targetFramework;
        private CancellationTokenSource _restoreCts;
        private string? _frameworkVersion;

        public ImmutableArray<string> LocalLibraryPaths { get; private set; }


        public string Id { get; set; }

        public string RestorePath { get; set; }
        public bool IsRestoring
        {
            get => _isRestoring;
            private set => SetProperty(ref _isRestoring, value);
        }
        public bool RestoreFailed
        {
            get => _restoreFailed;
            private set => SetProperty(ref _restoreFailed, value);
        }
        public Task RestoreTask { get; private set; }

        public IReadOnlyList<string> RestoreErrors
        {
            get => _restoreErrors;
            private set => SetProperty(ref _restoreErrors, value);
        }
        public PackageViewModel(ILogger logger, NugetPackage nugetPackage)
        {
            Id = Guid.NewGuid().ToString();
            _logger = logger;
            _nugetPackage = nugetPackage;
            LocalLibraryPaths = ImmutableArray<string>.Empty;
            _restoreLock = new SemaphoreSlim(1, 1);
            _libraries        = new HashSet<LibraryRef>();

            var framework = Assembly
                .GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            var str = framework?.Split(",Version=v");
            Debug.Assert(str != null, nameof(str) + " != null");
            var frameworkName = string.Concat(str);
            _targetFramework = NuGetFramework.ParseFolder(frameworkName);
            _frameworkVersion = str?[1];
        }
        public void SetTargetFramework(string targetFrameworkMoniker, string? frameworkVersion = null)
        {
            _targetFramework  = NuGetFramework.ParseFolder(targetFrameworkMoniker);
            _frameworkVersion = frameworkVersion;
            RefreshPackages();
        }
        public void UpdateLibraries(IReadOnlyList<LibraryRef> libraries)
        {
            var changed = false;

            if (_libraries.Count > 0 && (libraries == null || libraries.Count == 0))
            {
                _libraries.Clear();
                LocalLibraryPaths = ImmutableArray<string>.Empty;

                changed = true;
            }
            else
            {
                var removed = _libraries.RemoveWhere(p =>
                {
                    var remove = !libraries.Contains(p);
                    if (remove && p.Path != null)
                    {
                        LocalLibraryPaths = LocalLibraryPaths.Remove(p.Path);
                    }

                    return remove;
                });

                if (removed > 0)
                {
                    changed = true;
                }

                if (libraries != null)
                {
                    foreach (var library in libraries)
                    {
                        if (_libraries.Add(library))
                        {
                            if (library.Path != null)
                            {
                                LocalLibraryPaths = LocalLibraryPaths.Add(library.Path);
                            }

                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                RefreshPackages();
            }
        }
        private void RefreshPackages()
        {
            if (RestorePath == null || _targetFramework == null) return;

            _restoreCts?.Cancel();

            var packages = _libraries?.ToArray();

            var restoreCts        = new CancellationTokenSource();
            var cancellationToken = restoreCts.Token;
            _restoreCts = restoreCts;

            RestoreTask = Task.Run(() => RefreshPackagesAsync(packages, cancellationToken), cancellationToken);
        }
        private async Task RefreshPackagesAsync(LibraryRef[]? libraries, CancellationToken cancellationToken)
        {
            await _restoreLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            IsRestoring = true;
            // try
            {
                var restoreParams = _nugetPackage.CreateRestoreParams();
                restoreParams.ProjectName      = Id;
                restoreParams.OutputPath       = RestorePath;
                restoreParams.Libraries        = libraries;
                restoreParams.TargetFramework  = _targetFramework;
                restoreParams.FrameworkVersion = _frameworkVersion;

                var lockFilePath = Path.Combine(RestorePath, "project.assets.json");
                
                if(File.Exists(lockFilePath)) File.Delete(lockFilePath);
                    
                var result = await PackageUtils.RestoreAsync(restoreParams, NullLogger.Instance, cancellationToken).ConfigureAwait(false);

                if (!result.Success)
                {
                    RestoreFailed = true;
                    RestoreErrors = result.Errors;
                    return;
                }

                RestoreFailed = false;
                RestoreErrors = Array.Empty<string>();

                if (result.NoOp)
                {
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                _nugetPackage.ParseLockFile(lockFilePath, cancellationToken, _targetFramework, _frameworkVersion, _libraries);

            }
            // catch (Exception e) when (!(e is OperationCanceledException))
            // {
            //     _logger?.LogError(e.Message + e.StackTrace);
            //
            // }
            // finally
            // {
            //     _restoreLock.Release();
            //     IsRestoring = false;
            // }
        }



    }
}
