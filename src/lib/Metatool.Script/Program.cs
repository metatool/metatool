using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Metatool.Metatool.SimpleConsoleLoggerProvider;

namespace Metatool.Script
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new SimpleConsoleLogger(nameof(ScriptHost));
            var subCmd = args[0];
            if (subCmd.EndsWith(".csx"))
            {
                var scriptPath = subCmd;
                if (!File.Exists(scriptPath))
                {
                    logger.LogError("the script path is not right!");
                    return;
                }

                var assemblyName = Path.GetFileNameWithoutExtension(scriptPath);
                for (var i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-n")
                    {
                        assemblyName = args[i + 1];
                        i++;
                    }
                    // "--" is handled internally
                    else
                    {
                        logger.LogError("unexpected argument.");
                        return;
                    }
                }

                var outputDir = Path.Combine(Path.GetDirectoryName(scriptPath), "bin");
                var dll = Path.Combine(outputDir, assemblyName + ".dll");

                await new ScriptHost(logger).Build(scriptPath, outputDir, assemblyName, onlyBuild: true/*true: to debug, we should not run it in it's own process*/);
                var assembly = Assembly.LoadFrom(dll);
                assembly.EntryPoint?.Invoke(null, new string[] { });
                return;
            }
            else if (subCmd == "init")
            {

            }
            else
            {
                logger.LogError("unexpected argument");
                return;

            }

        }
    }
}
