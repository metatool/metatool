using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Metatool.Metaing
{
    class PluginLoadContext : AssemblyLoadContext
    {
        private  AssemblyDependencyResolver _resolver;
        readonly Assembly[]                 _assemblies = AppDomain.CurrentDomain.GetAssemblies();

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                var ab = LoadFromAssemblyPath(assemblyPath);
                // solve problem of ILogger loaded from different location, i.e. global assembly cache and packages folder
                var assembly = _assemblies.FirstOrDefault(a => a.FullName.Equals(ab.FullName));
                if (assembly != null)
                {
                    return assembly;
                }

                return ab;
            }

            // all the dependency assemblies are loaded into the default context and only the assemblies explicitly loaded into the new context are in this context.
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
