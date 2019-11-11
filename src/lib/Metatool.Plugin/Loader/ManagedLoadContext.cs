﻿// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Metatool.Metatool.Plugin.LibraryModel;

namespace Metatool.Metatool.Plugin.Loader
{
    [DebuggerDisplay("'{Name}' ({_mainAssemblyPath})")]
    internal class ManagedLoadContext : AssemblyLoadContext
    {
        private readonly string _basePath;
        private readonly string _mainAssemblyPath;
        private readonly IReadOnlyDictionary<string, ManagedLibrary> _managedAssemblies;
        private readonly IReadOnlyDictionary<string, NativeLibrary> _nativeLibraries;
        private readonly IReadOnlyCollection<string> _privateAssemblies;
        private readonly IReadOnlyCollection<string> _defaultAssemblies;
        private readonly ICollection<string> _sharedAssemblyPrefixes;
        private readonly IReadOnlyCollection<string> _additionalProbingPaths;
        private readonly bool _preferDefaultLoadContext;
        private readonly string[] _resourceRoots;
        private readonly AssemblyDependencyResolver _dependencyResolver;

        public ManagedLoadContext(string mainAssemblyPath,
            IReadOnlyDictionary<string, ManagedLibrary> managedAssemblies,
            IReadOnlyDictionary<string, NativeLibrary> nativeLibraries,
            IReadOnlyCollection<string> privateAssemblies,
            IReadOnlyCollection<string> defaultAssemblies,
            IReadOnlyCollection<string> additionalProbingPaths,
            IReadOnlyCollection<string> resourceProbingPaths,
            bool preferDefaultLoadContext,
            bool isCollectible, ICollection<string> sharedAssemblyPrefixes)
            : base(Path.GetFileNameWithoutExtension(mainAssemblyPath), isCollectible)
        {
            if (resourceProbingPaths == null)
            {
                throw new ArgumentNullException(nameof(resourceProbingPaths));
            }

            _mainAssemblyPath = mainAssemblyPath ?? throw new ArgumentNullException(nameof(mainAssemblyPath));
            _dependencyResolver = new AssemblyDependencyResolver(mainAssemblyPath);
            _basePath = Path.GetDirectoryName(mainAssemblyPath) ?? throw new ArgumentException(nameof(mainAssemblyPath));
            _managedAssemblies = managedAssemblies ?? throw new ArgumentNullException(nameof(managedAssemblies));
            _privateAssemblies = privateAssemblies ?? throw new ArgumentNullException(nameof(privateAssemblies));
            _defaultAssemblies = defaultAssemblies ?? throw new ArgumentNullException(nameof(defaultAssemblies));
            _nativeLibraries = nativeLibraries ?? throw new ArgumentNullException(nameof(nativeLibraries));
            _additionalProbingPaths = additionalProbingPaths ?? throw new ArgumentNullException(nameof(additionalProbingPaths));
            _preferDefaultLoadContext = preferDefaultLoadContext;
            _sharedAssemblyPrefixes = sharedAssemblyPrefixes;

            _resourceRoots = new[] { _basePath }
                .Concat(resourceProbingPaths)
                .ToArray();
        }

        /// <summary>
        /// Load an assembly.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns>
        /// null: all the dependency assemblies are loaded into the default context and only the assemblies explicitly loaded into the new context are in this context.
        /// </returns>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == null)
            {
                // not sure how to handle this case. It's technically possible.
                return null;
            }

            if ((_preferDefaultLoadContext ||_sharedAssemblyPrefixes.Any(p=>assemblyName.Name.StartsWith(p))|| _defaultAssemblies.Contains(assemblyName.Name)) && !_privateAssemblies.Contains(assemblyName.Name))
            {
                // If default context is preferred, check first for types in the default context unless the dependency has been declared as private
                try
                {
                    var defaultAssembly = Default.LoadFromAssemblyName(assemblyName);
                    if (defaultAssembly != null)
                    {
                        // return null so ALC will fallback to loading from Default ALC directly
                        return null;
                    }
                }
                catch
                {
                    // Swallow errors in loading from the default context
                }
            }

            var resolvedPath = _dependencyResolver.ResolveAssemblyToPath(assemblyName);
            if (!string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath))
            {
                return LoadFromAssemblyPath(resolvedPath);
            }

            // Resource assembly binding does not use the TPA. Instead, it probes PLATFORM_RESOURCE_ROOTS (a list of folders)
            // for $folder/$culture/$assemblyName.dll
            // See https://github.com/dotnet/coreclr/blob/3fca50a36e62a7433d7601d805d38de6baee7951/src/binder/assemblybinder.cpp#L1232-L1290

            if (!string.IsNullOrEmpty(assemblyName.CultureName) && !string.Equals("neutral", assemblyName.CultureName))
            {
                foreach (var resourceRoot in _resourceRoots)
                {
                    var resourcePath = Path.Combine(resourceRoot, assemblyName.CultureName, assemblyName.Name + ".dll");
                    if (File.Exists(resourcePath))
                    {
                        return LoadFromAssemblyPath(resourcePath);
                    }
                }

                return null;
            }

            if (_managedAssemblies.TryGetValue(assemblyName.Name, out var library) && library != null)
            {
                if (SearchForLibrary(library, out var path) && path != null)
                {
                    return LoadFromAssemblyPath(path);
                }
            }
            else
            {
                // if an assembly was not listed in the list of known assemblies,
                // fallback to the load context base directory
                var localFile = Path.Combine(_basePath, assemblyName.Name + ".dll");
                if (File.Exists(localFile))
                {
                    return LoadFromAssemblyPath(localFile);
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the unmanaged binary using configured list of native libraries.
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <returns></returns>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var resolvedPath = _dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (!string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath))
            {
                return LoadUnmanagedDllFromPath(resolvedPath);
            }

            foreach (var prefix in PlatformInformation.NativeLibraryPrefixes)
            {
                if (_nativeLibraries.TryGetValue(prefix + unmanagedDllName, out var library))
                {
                    if (SearchForLibrary(library, prefix, out var path) && path != null)
                    {
                        return LoadUnmanagedDllFromResolvedPath(path);
                    }
                }
                else
                {
                    // coreclr allows code to use [DllImport("sni")] or [DllImport("sni.dll")]
                    // This library treats the file name without the extension as the lookup name,
                    // so this loop is necessary to check if the unmanaged name matches a library
                    // when the file extension has been trimmed.
                    foreach (var suffix in PlatformInformation.NativeLibraryExtensions)
                    {
                        if (!unmanagedDllName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // check to see if there is a library entry for the library without the file extension
                        var trimmedName = unmanagedDllName.Substring(0, unmanagedDllName.Length - suffix.Length);

                        if (_nativeLibraries.TryGetValue(prefix + trimmedName, out library))
                        {
                            if (SearchForLibrary(library, prefix, out var path) && path != null)
                            {
                                return LoadUnmanagedDllFromResolvedPath(path);
                            }
                        }
                        else
                        {
                            // fallback to native assets which match the file name in the plugin base directory
                            var localFile = Path.Combine(_basePath, prefix + unmanagedDllName + suffix);
                            if (File.Exists(localFile))
                            {
                                return LoadUnmanagedDllFromResolvedPath(localFile);
                            }

                            var localFileWithoutSuffix = Path.Combine(_basePath, prefix + unmanagedDllName);
                            if (File.Exists(localFileWithoutSuffix))
                            {
                                return LoadUnmanagedDllFromResolvedPath(localFileWithoutSuffix);
                            }
                        }
                    }

                }
            }

            return base.LoadUnmanagedDll(unmanagedDllName);
        }

        private bool SearchForLibrary(ManagedLibrary library, out string? path)
        {
            // 1. Check for in _basePath + app local path
            var localFile = Path.Combine(_basePath, library.AppLocalPath);
            if (File.Exists(localFile))
            {
                path = localFile;
                return true;
            }

            // 2. Search additional probing paths
            foreach (var searchPath in _additionalProbingPaths)
            {
                var candidate = Path.Combine(searchPath, library.AdditionalProbingPath);
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }

            // 3. Search in base path
            foreach (var ext in PlatformInformation.ManagedAssemblyExtensions)
            {
                var local = Path.Combine(_basePath, library.Name.Name + ext);
                if (File.Exists(local))
                {
                    path = local;
                    return true;
                }
            }

            path = null;
            return false;
        }

        private bool SearchForLibrary(NativeLibrary library, string prefix, out string? path)
        {
            // 1. Search in base path
            foreach (var ext in PlatformInformation.NativeLibraryExtensions)
            {
                var candidate = Path.Combine(_basePath, $"{prefix}{library.Name}{ext}");
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }

            // 2. Search in base path + app local (for portable deployments of netcoreapp)
            var local = Path.Combine(_basePath, library.AppLocalPath);
            if (File.Exists(local))
            {
                path = local;
                return true;
            }

            // 3. Search additional probing paths
            foreach (var searchPath in _additionalProbingPaths)
            {
                var candidate = Path.Combine(searchPath, library.AdditionalProbingPath);
                if (File.Exists(candidate))
                {
                    path = candidate;
                    return true;
                }
            }

            path = null;
            return false;
        }

        private IntPtr LoadUnmanagedDllFromResolvedPath(string unmanagedDllPath)
        {
            var normalized = Path.GetFullPath(unmanagedDllPath);
            return LoadUnmanagedDllFromPath(normalized);
        }
    }
}
