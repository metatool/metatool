using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Metaseed.MetaPlugin;

namespace Metaseed.Metaing
{
    internal class PluginLoad
    {
        static List<string> GetPluginPaths()
        {
            var plugins    = new List<string>();
            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName   = Path.GetFileName(dir);
                var pluginDll = Path.GetFullPath(Path.Combine(dir, dirName + ".dll"));
                if (File.Exists(pluginDll))
                    plugins.Add(pluginDll);
            }

            return plugins;
        }

        private static IEnumerable<IMetaPlugin> GetPlugin(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IMetaPlugin).IsAssignableFrom(type))
                {
                    object      o      = Activator.CreateInstance(type);
                    IMetaPlugin plugin = o as IMetaPlugin;
                    if (plugin != null) yield return plugin;
                }
            }
        }

        internal static void Load()
        {
            try
            {
                var pluginPaths = GetPluginPaths();
                var plugins = pluginPaths.SelectMany(p =>
                {
                    var context  = new PluginLoadContext(p);
                    var assembly = context.LoadFromAssemblyPath(p);
                    return GetPlugin(assembly);
                });

                foreach (var plugin in plugins)
                {
                    plugin.Init();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}