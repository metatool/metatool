using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Metaseed.Metatool.Plugin.Loader;

namespace Metaseed.Metatool.Plugin
{
    /// <summary>
    /// This loader attempts to load binaries for execution (both managed assemblies and native libraries)
    /// in the same way that .NET Core would if they were originally part of the .NET Core application.
    /// <para>
    /// This loader reads configuration files produced by .NET Core (.deps.json and runtimeconfig.json)
    /// as well as a custom file (*.config files). These files describe a list of .dlls and a set of dependencies.
    /// The loader searches the plugin path, as well as any additionally specified paths, for binaries
    /// which satisfy the plugin's requirements.
    /// <seealso href="https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md">
    /// https://github.com/natemcmaster/DotNetCorePlugins/blob/master/docs/what-are-shared-types.md
    /// </seealso>
    /// </para>
    /// </summary>
    public class PluginLoader : IDisposable
    {
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, _ => { });

        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes,
            Action<PluginConfig> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                sharedTypes,
                config =>
                {
                    config.IsUnloadable = isUnloadable;
                    configure(config);
                });
        }

        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });

        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes,
            Action<PluginConfig> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                config =>
                {
                    if (sharedTypes != null)
                    {
                        foreach (var type in sharedTypes)
                        {
                            config.SharedAssemblies.Add(type.Assembly.GetName());
                        }
                    }

                    configure(config);
                });
        }

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile)
            => CreateFromAssemblyFile(assemblyFile, _ => { });

        /// <summary>
        /// Create a plugin loader for an assembly file.
        /// </summary>
        /// <param name="assemblyFile">The file path to the main assembly for the plugin.</param>
        /// <param name="configure">A function which can be used to configure advanced options for the plugin loader.</param>
        /// <returns>A loader.</returns>
        public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Action<PluginConfig> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var config = new PluginConfig(assemblyFile);
            configure(config);
            return new PluginLoader(config);
        }

        private readonly PluginConfig        _config;
        private readonly AssemblyLoadContext _context;
        private volatile bool                _disposed;

        /// <summary>
        /// Initialize an instance of <see cref="PluginLoader" />
        /// </summary>
        /// <param name="config">The configuration for the plugin.</param>
        public PluginLoader(PluginConfig config)
        {
            _config  = config ?? throw new ArgumentNullException(nameof(config));
            _context = CreateLoadContext(config);
        }

        /// <summary>
        /// True when this plugin is capable of being unloaded.
        /// </summary>
        public bool IsUnloadable
        {
            get { return _context.IsCollectible; }
        }

        internal AssemblyLoadContext LoadContext => _context;

        /// <summary>
        /// Load the main assembly for the plugin.
        /// </summary>
        public Assembly LoadDefaultAssembly()
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyPath(_config.MainAssemblyPath);
        }

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(AssemblyName assemblyName)
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyName(assemblyName);
        }

        /// <summary>
        /// Load an assembly from path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssemblyFromPath(string assemblyPath)
            => _context.LoadFromAssemblyPath(assemblyPath);

        /// <summary>
        /// Load an assembly by name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The assembly.</returns>
        public Assembly LoadAssembly(string assemblyName)
        {
            EnsureNotDisposed();
            return LoadAssembly(new AssemblyName(assemblyName));
        }

        /// <summary>
        /// Disposes the plugin loader. This only does something if <see cref="IsUnloadable" /> is true.
        /// When true, this will unload assemblies which which were loaded during the lifetime
        /// of the plugin.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_context.IsCollectible)
            {
                _context.Unload();
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PluginLoader));
            }
        }

        private static AssemblyLoadContext CreateLoadContext(PluginConfig config)
        {
            var builder = new AssemblyLoadContextBuilder();

            builder.SetMainAssemblyPath(config.MainAssemblyPath);

            foreach (var ext in config.PrivateAssemblies)
            {
                builder.PreferLoadContextAssembly(ext);
            }

            if (config.PreferSharedTypes)
            {
                builder.PreferDefaultLoadContext(true);
            }

            if (config.IsUnloadable)
            {
                builder.EnableUnloading();
            }

            foreach (var assemblyName in config.SharedAssemblies)
            {
                builder.PreferDefaultLoadContextAssembly(assemblyName);
            }


            return builder.Build();
        }
    }
}