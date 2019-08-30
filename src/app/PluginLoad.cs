using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Metaseed.MetaPlugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Metaseed.Metaing
{
    internal class PluginLoad
    {
        static List<string> GetPluginDlls()
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

        private static void GetPlugin(Assembly assembly, IServiceCollection services)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IMetaPlugin).IsAssignableFrom(type))
                {
                    services.AddSingleton(typeof(IMetaPlugin), type);
                    //object      o      = Activator.CreateInstance(type);
                    //IMetaPlugin plugin = o as IMetaPlugin;
                    //if (plugin != null) yield return plugin;
                }
            }
        }

        internal static void Load(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                var pluginPaths = GetPluginDlls();
                pluginPaths.ForEach(p =>
                {
                    var context  = new PluginLoadContext(p);
                    var assembly = context.LoadFromAssemblyPath(p);
                    GetPlugin(assembly, services);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}