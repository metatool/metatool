using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using McMaster.Extensions.CommandLineUtils;
using Metatool.Plugin;
using Metatool.Utils;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public class Scaffolder
    {
        private readonly ILogger       _logger;
        private readonly CommandRunner _commandRunner;

        public Scaffolder(ILogger logger)
        {
            _logger        = logger;
            _commandRunner = new CommandRunner(logger);
        }

        public void RegisterFileHandler()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // register dotnet-script as the tool to process .csx files
                _commandRunner.Execute("reg", @"add HKCU\Software\classes\.csx /f /ve /t REG_SZ /d metatool");
                _commandRunner.Execute("reg",
                    $@"add HKCU\Software\Classes\metatool\Shell\Open\Command /f /ve /t REG_EXPAND_SZ /d ""\""%MetatoolDir%\Metatool.exe\"" \""%1\"" -- %*""");
            }
        }

        public void SetupEnvVar()
        {
            var value = Environment.GetEnvironmentVariable("MetatoolDir");
            if (value != null && value == Context.AppDirectory) return;

            Environment.SetEnvironmentVariable("MetatoolDir", Context.AppDirectory,
                EnvironmentVariableTarget.User);
            _logger.LogInformation($"Set User Environment Var: MetatoolDir = {Context.AppDirectory}");
        }

        public string AddToPath(EnvironmentVariableTarget target)
        {
            var s     = System.Environment.GetEnvironmentVariable("PATH", target);
            var paths = s.Split(Path.PathSeparator).ToList();

            if (paths.Contains(Context.AppDirectory, StringComparer.InvariantCultureIgnoreCase)) return s;

            s = $"{AppContext.BaseDirectory}{Path.PathSeparator}{s}";

            try
            {
                System.Environment.SetEnvironmentVariable("PATH", s, target);
                _logger.LogInformation($"Add to {target} PATH Environment Var.");
            }
            catch (SecurityException)
            {
                _logger.LogWarning("Could not add to Machine scale Path environment variable, run as Admin to set it!");
            }

            return s;
        }

        public void InitTemplate(string toolName, string dir = null, bool isScript = true)
        {
            var resource = isScript
                ? "Metaseed.Metatool.Templates.ToolTemplate.zip"
                : "Metaseed.Metatool.Templates.LibTemplate.zip";
            dir ??= Path.Combine(Context.CurrentDirectory, toolName);
            if (Directory.Exists(dir))
            {
                if (!Prompt.GetYesNo($"We already have a same folder at: {dir}, do you want to override?", false,
                    ConsoleColor.Yellow))
                {
                    Console.WriteLine("command canceled.");
                    return;
                }
            }
            using var stream =
                typeof(Scaffolder).Assembly.GetManifestResourceStream(resource);
            new ZipArchive(stream).ExtractToDirectory(dir,true);
            _logger.LogInformation($"Metatool: tool {toolName} is created in folder: {dir}");
            Explorer.Open(dir);
        }
    }
}