using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Metatool.Utils;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public class Scaffolder
    {
        private readonly ILogger _logger;
        private readonly CommandRunner _commandRunner;

        public Scaffolder(ILogger logger)
        {
            _logger = logger;
            _commandRunner = new CommandRunner(logger);
        }
        public void RegisterFileHandler()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // register dotnet-script as the tool to process .csx files
                _commandRunner.Execute("reg", @"add HKCU\Software\classes\.csx /f /ve /t REG_SZ /d metatool");
                _commandRunner.Execute("reg", $@"add HKCU\Software\Classes\metatool\Shell\Open\Command /f /ve /t REG_EXPAND_SZ /d ""\""%MetatoolDir%\Metatool.exe\"" \""%1\"" -- %*""");
            }
        }

        public string AddToPath(EnvironmentVariableTarget target)
        {
            var s     = System.Environment.GetEnvironmentVariable("PATH", target);
            var paths = s.Split(Path.PathSeparator).ToList();
            if (!paths.Contains(AppContext.BaseDirectory, StringComparer.InvariantCultureIgnoreCase))
            {
                s = $"{AppContext.BaseDirectory}{Path.PathSeparator}{s}";
                System.Environment.SetEnvironmentVariable("PATH", s, target);
                _logger.LogInformation($"Add to {target} PATH Environment Var.");
            }

            return s;
        }

        public void InitScriptTemplate(string toolName, string dir=null)
        {
            dir??=Path.Combine(AppContext.BaseDirectory, "tools", toolName);
            var zipPath = Path.Combine(AppContext.BaseDirectory, "ToolTemplate.zip");
            ZipFile.ExtractToDirectory(zipPath, dir , true);
            _logger.LogInformation($"Metatool Script: {toolName} is created in folder: {dir}");
        }

    }
}
