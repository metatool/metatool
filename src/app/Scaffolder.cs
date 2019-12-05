using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using McMaster.Extensions.CommandLineUtils;
using Metatool.Service;
using Metatool.Utils;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public class Scaffolder
    {
        private readonly ILogger        _logger;
        private readonly IFileExplorer  _fileExplorer;
        private          ICommandRunner _commandRunner;
        private FunctionalKeys _functions;

        public Scaffolder(ILogger logger)
        {
            _logger        = logger;
            _fileExplorer  = Services.Get<IFileExplorer>();
            _commandRunner = Services.Get<ICommandRunner>();
        }

        public void RegisterFileHandler()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // register dotnet-script as the tool to process .csx files
                var cmdRunner = new CommandRunner();
                cmdRunner.Run("reg", @"add HKCU\Software\classes\.csx /f /ve /t REG_SZ /d metatool");
                cmdRunner.Run("reg",
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

        public void SetupFunctions(IConfig<MetatoolConfig> config)
        {
            _functions = new FunctionalKeys(config);
        }

        public string AddToPath(EnvironmentVariableTarget target)
        {
            var s     = System.Environment.GetEnvironmentVariable("PATH", target);
            var paths = s.Split(Path.PathSeparator).ToList();

            if (paths.Any(p=>StringComparer.InvariantCultureIgnoreCase.Compare(Context.AppDirectory, p)!=0)) return s;

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
                ? "Metaseed.Metatool.Templates.Metatool.Tools.ScriptTool.zip"
                : "Metaseed.Metatool.Templates.Metatool.Tools.LibTool.zip";
            dir??=Path.Combine(Context.CurrentDirectory, toolName);
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
            new ZipArchive(stream).ExtractToDirectory(dir, true);
            _logger.LogInformation($"Metatool: tool {toolName} is created in folder: {dir}");
            try
            {
                _commandRunner.RunWithCmd($"code {dir}");
                _logger.LogInformation("open it with vscode...");
            }
            catch (Exception e)
            {
                _fileExplorer.Open(dir);
                _logger.LogWarning(e.Message);
            }
        }
    }
}