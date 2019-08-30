using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Metaseed.Metaing
{
    class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
           
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var ab = LoadFromAssemblyPath(assemblyPath);
                // solve problem of ILogger loaded from different location, i.e. global assembly cache and packages folder
                var assembly = assemblies.FirstOrDefault(a => a.FullName.Equals(ab.FullName));
                if (assembly != null)
                {
                    return assembly;
                }

                return ab;

            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
