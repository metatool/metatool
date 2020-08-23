using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Script;
using Metatool.Service;
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
                if (args.Length == 0)
                {
                    logger.LogInformation(@$"v{Assembly.GetEntryAssembly().GetName().Version} © metaseed
* run script
  cs <scriptPath> [-- arg0 arg1...]
* init script
  cs init
");
                    return;
                }

                var subCmd = args[0];
                if (subCmd.EndsWith(".csx"))
                {
                    await ScriptCommand(args, logger);
                    return;
                }
                else if (subCmd == "init")
                {
                    InitCommand(args, logger);
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

            var       outputDir         = Path.Combine(Path.GetDirectoryName(scriptPath)!, "bin");
            var       dll               = Path.Combine(outputDir, assemblyName + ".dll");
            using var executeCts        = new CancellationTokenSource();
            var       cancellationToken = executeCts.Token;
            await new ScriptHost(logger).Build(scriptPath, outputDir, assemblyName, OptimizationLevel.Debug,
                cancellationToken);
            var assembly = Assembly.LoadFrom(dll);
            assembly.EntryPoint?.Invoke(null, new object[] { });
        }

        static void InitCommand(string[] args, ILogger logger)
        {
            try
            {
                var scriptsFolderName = args[1];
                var dir               = Environment.CurrentDirectory;
                for (var i = 2; i < args.Length; i++)
                {
                    if (args[i] == "-d")
                    {
                        dir = args[++i];
                    }
                }

                if (!Directory.Exists(dir))
                {
                    logger.LogError($"the script target path({dir}) is not right! ");
                    return;
                }

                InitTemplate(logger, scriptsFolderName, dir);
            }
            catch
            {
                logger.LogError("cs init <scriptsFolderName> [-d targetDir]");
            }
        }

        static void InitTemplate(ILogger logger, string scriptName, string dir = null)
        {
            var resource = "Metaseed.CSharpScript.Templates.Script.zip";
            dir ??= Context.CurrentDirectory;
            dir =   Path.Combine(dir, scriptName);
            if (Directory.Exists(dir))
            {
                if (!Utils.GetYesNo($"We already have a same folder at: {dir}, do you want to override?", false,
                    ConsoleColor.Yellow))
                {
                    Console.WriteLine("command canceled.");
                    return;
                }
            }

            using var stream =
                typeof(Program).Assembly.GetManifestResourceStream(resource);
            new ZipArchive(stream).ExtractToDirectory(dir, true);
            logger.LogInformation($"cs: {scriptName} is created in folder: {dir}");
            try
            {
                var cmdWithArgs = $"code {dir}";
                var proc = new Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName               = "cmd.exe",
                        Arguments              = "/C \"" + cmdWithArgs + "\"",
                        UseShellExecute        = false,
                        CreateNoWindow         = true,
                        WindowStyle            = ProcessWindowStyle.Hidden,
                        WorkingDirectory       = dir ?? Context.AppDirectory,
                        RedirectStandardInput  = true,
                        RedirectStandardOutput = true,
                    }
                }.Start();
                logger.LogInformation("open it with vscode...");
            }
            catch (Exception e)
            {
                Process.Start("explorer.exe", dir);
                logger.LogWarning(e.Message);
            }
        }
    }
}