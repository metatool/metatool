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
    public class Scaffold
    {
        private readonly ILogger _logger;
        private readonly IFileExplorer _fileExplorer;
        private IShell _shell;
        private FunctionalKeys _functions;

        public Scaffold(ILogger logger)
        {
            _logger = logger;
            _fileExplorer = Services.Get<IFileExplorer>();
            _shell = Services.Get<IShell>();
        }

        public void Register()
        {
            CreateShortcut();
            AddToPath(EnvironmentVariableTarget.User);
            AddToPath(EnvironmentVariableTarget.Machine);
            SetupEnvVar();
            RegisterFileHandler();
        }

        void RegisterFileHandler()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // register dotnet-script as the tool to process .csx files
                var cmdRunner = _shell;
                cmdRunner.Run("reg", @"add HKCU\Software\classes\.csx /f /ve /t REG_SZ /d metatool");
                cmdRunner.Run("reg",
                    $@"add HKCU\Software\Classes\metatool\Shell\Open\Command /f /ve /t REG_EXPAND_SZ /d ""\""%MetatoolDir%\Metatool.exe\"" \""%1\"" -- %*""");
            }
        }

        void SetupEnvVar()
        {
            var value = Environment.GetEnvironmentVariable("MetatoolDir");
            if (value != null && value == Context.AppDirectory) return;

            Environment.SetEnvironmentVariable("MetatoolDir", Context.AppDirectory,
                EnvironmentVariableTarget.User);
            _logger.LogInformation($"Set User Environment Var: MetatoolDir = {Context.AppDirectory}");
        }

        public void CommonSetup(IConfig<MetatoolConfig> config)
        {
            _functions = new FunctionalKeys(config);
        }

        string AddToPath(EnvironmentVariableTarget target)
        {
            var s = System.Environment.GetEnvironmentVariable("PATH", target);
            if (s != null)
            {
                var paths = s.Split(Path.PathSeparator).ToList();

                if (paths.Any(p =>
                    StringComparer.InvariantCultureIgnoreCase.Compare(Context.AppDirectory, p) == 0))
                    return s;
            }

            s = $"{Context.AppDirectory}{Path.PathSeparator}{s}";

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
                typeof(Scaffold).Assembly.GetManifestResourceStream(resource);
            new ZipArchive(stream).ExtractToDirectory(dir, true);
            _logger.LogInformation($"Metatool: tool {toolName} is created in folder: {dir}");
            try
            {
                _shell.RunWithCmd($"code {dir}");
                _logger.LogInformation("open it with vscode...");
            }
            catch (Exception e)
            {
                _fileExplorer.Open(dir);
                _logger.LogWarning(e.Message);
            }
        }

        void CreateShortcut()
        {
            var description = "Metatool for your professional life";
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var targetPath = Path.Combine(Context.AppDirectory, "Metatool.exe");
            var shortcutPath = Path.Combine(desktop, "Metatool.lnk");
            var shortcutPathAdmin = Path.Combine(desktop, "Metatool (Admin).lnk");

            var shell = _shell;
            var shortcut = shell.ReadShortcut(shortcutPath);
            var shortcutAdmin = shell.ReadShortcut(shortcutPathAdmin);

            if (shortcut.TargetPath != targetPath)
                shell.CreateShortcut(targetPath, shortcutPath, "Ctrl+Alt+X", description);
            if (shortcutAdmin.TargetPath != targetPath)
                shell.CreateShortcut(targetPath, shortcutPathAdmin, "Ctrl+Alt+Z", description + "- Admin", true);
        }
    }
}