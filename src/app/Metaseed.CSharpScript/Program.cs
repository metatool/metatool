using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Script;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using static Metatool.Metatool.SimpleConsoleLoggerProvider;

namespace Metaseed.CSharpScript
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new SimpleConsoleLogger(nameof(ScriptHost));
            try
            {
                var subCmd = args[0];
                if (subCmd.EndsWith(".csx"))
                {
                    await ScriptCommand(args, logger);
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
            catch (Exception e)
            {
                logger.LogError(e, "Error");
            }
        }

        static async Task ScriptCommand(string[] args, ILogger logger)
        {
            var scriptPath = args[0];
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

            var outputDir = Path.Combine(Path.GetDirectoryName(scriptPath)!, "bin");
            var dll = Path.Combine(outputDir, assemblyName + ".dll");
            using var executeCts = new CancellationTokenSource();
            var cancellationToken = executeCts.Token;
            await new ScriptHost(logger).Build(scriptPath, outputDir, assemblyName, OptimizationLevel.Debug, cancellationToken);
            var assembly = Assembly.LoadFrom(dll);
            assembly.EntryPoint?.Invoke(null, new object[] { });
        }
    }
}
