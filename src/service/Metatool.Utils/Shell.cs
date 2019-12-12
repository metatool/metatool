using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace Metatool.Utils
{
    public class Shell : IShell
    {
        private readonly ILogger _logger = Services.Get<ILogger<Shell>>();

        /// <summary>
        /// in it's own process without window
        /// </summary>
        public CommandResult Run(string commandPath, string arguments, string workingDirectory = null)
        {
           var startInformation = new ProcessStartInfo($"{commandPath}")
            {
                CreateNoWindow         = true,
                Arguments              = arguments ?? "",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                WorkingDirectory       = workingDirectory ?? System.Environment.CurrentDirectory
            }; ;
            var process          = new Process { StartInfo = startInformation };
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    _logger.LogDebug(e.Data);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    _logger.LogDebug(e.Data);
                }
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            var standardOut   = process.StandardOutput.ReadToEnd();
            if(!string.IsNullOrEmpty(standardOut)) _logger.LogInformation(standardOut);
            var standardError = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(standardError)) _logger.LogWarning(standardError);

            process.WaitForExit();
            return new CommandResult(process.ExitCode, standardOut, standardError);
        }
      
        // i.e. if run as admin Chrome could not load extensions
        public void RunAsNormalUser(string cmd, params string[] args)
        {
            if (Context.IsElevated)
            {
                var c = args.ToList();
                    c.Insert(0, cmd);
                var exeWithArgs =  NormalizeCmd(c.ToArray());
                var s = $"start \"\" {exeWithArgs} \n exit 0";
                var tempBat = Path.Combine(Path.GetTempPath(), "t.bat");
                File.WriteAllText(tempBat, s);
                RunWithExplorer(tempBat);
            }
            else
            {
                new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName        = cmd,
                        Arguments= NormalizeCmd(args)
                    }
                }.Start();
            }
           
        }

        private readonly char[] _specialChars = new[] {' ', '&', '<', '>', '[', ']', '{', '}', '^', '=', ';', '!', '\'', '+',',', '`', '~'};
        public string NormalizeCmd(params string[] cmdArgsSerials)
        {
            return string.Join(" ", cmdArgsSerials.Select(arg => arg.Any(_specialChars.Contains) ? $"\"{arg}\"" : arg));
        }

        // Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
        // so we use cmd to make a workaround
        // https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/cmd
        // https: //ss64.com/nt/cmd.html
        // could run *.lnk with args
        public void RunWithCmd(string cmdWithArgs, bool asAdmin = false, string workingDir = null)
        {
            var proc = new Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName               = "cmd.exe",
                    Arguments              = "/C \"" + cmdWithArgs + "\"",
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    WindowStyle            = ProcessWindowStyle.Hidden,
                    WorkingDirectory       = workingDir ?? Context.AppDirectory,
                    RedirectStandardInput  = true,
                    RedirectStandardOutput = true,
                }
            };
            if (asAdmin) proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        /// <summary>
        /// Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
        /// so we use explorer to make a workaround
        ///
        /// this could run *.lnk and *.bat
        /// </summary>
        public void RunWithExplorer(string filePath, string workingDir = null)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName         = "explorer.exe",
                    ArgumentList     = {filePath},
                    UseShellExecute  = false,
                    CreateNoWindow   = true,
                    WindowStyle      = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingDir ?? Context.AppDirectory
                }
            };

            proc.Start();
        }

        public void CreateShortcut(string targetPath, string shortcutPath, string hotkey = "",
            string description = "", bool isAdmin = false)
        {
            var exists = System.IO.File.Exists(shortcutPath);
            if (exists) return;

            var shDesktop       = (object)"Desktop";
            var shell           = new WshShell();
            var shortcutAddress = shortcutPath;
            var shortcut        = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = description;
            shortcut.Hotkey      = hotkey;
            shortcut.TargetPath  = targetPath;
            shortcut.Save();
            if (isAdmin)
            {
                // admin hack
                using var fs = new FileStream(shortcutPath, FileMode.Open, FileAccess.ReadWrite);
                fs.Seek(21, SeekOrigin.Begin);
                fs.WriteByte(0x22);
            }
        }
    }
}