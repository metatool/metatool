using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Metatool.Service;
using Microsoft.Extensions.Logging;

namespace Metatool.Utils
{
    public class CommandRunner : ICommandRunner
    {
        private static readonly ILogger _logger = Services.Get<ILogger<CommandRunner>>();


        public int Run(string commandPath, string arguments = null, string workingDirectory = null)
        {
            _logger.LogDebug($"Executing '{commandPath} {arguments}'");
            var startInformation = CreateProcessStartInfo(commandPath, arguments, workingDirectory);
            var process          = CreateProcess(startInformation);
            RunAndWait(process);
            return process.ExitCode;
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

        public CommandResult Capture(string commandPath, string arguments, string workingDirectory = null)
        {
            var startInformation = CreateProcessStartInfo(commandPath, arguments, workingDirectory);
            var process          = CreateProcess(startInformation);
            process.Start();
            var standardOut   = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new CommandResult(process.ExitCode, standardOut, standardError);
        }

        public string NormalizeCmd(params string[] cmdArgsSerials)
        {
            return string.Join(" ", cmdArgsSerials.Select(arg => arg.Any(char.IsWhiteSpace) ? $"\"{arg}\"" : arg));
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
                    RedirectStandardInput  = true,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow         = true,
                    WindowStyle            = ProcessWindowStyle.Hidden,
                    WorkingDirectory       = workingDir ?? Context.AppDirectory
                }
            };
            if (asAdmin) proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        /// <summary>
        /// Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
        /// so we use cmd to make a workaround
        ///
        /// this could run *.lnk and *.bat
        /// </summary>
        /// <param name="filePath"></param>
        public void RunWithExplorer(string filePath, string workingDir = null)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName         = "explorer.exe",
                    ArgumentList     = {filePath},
                    CreateNoWindow   = true,
                    UseShellExecute  = false,
                    WindowStyle      = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingDir ?? Context.AppDirectory
                }
            };

            proc.Start();
        }


        private static ProcessStartInfo CreateProcessStartInfo(string commandPath, string arguments,
            string workingDirectory)
        {
            var startInformation = new ProcessStartInfo($"{commandPath}")
            {
                CreateNoWindow         = true,
                Arguments              = arguments ?? "",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                WorkingDirectory       = workingDirectory ?? System.Environment.CurrentDirectory
            };

            return startInformation;
        }

        private static void RunAndWait(System.Diagnostics.Process process)
        {
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private static System.Diagnostics.Process CreateProcess(ProcessStartInfo startInformation)
        {
            var process = new System.Diagnostics.Process {StartInfo = startInformation};
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
            return process;
        }
    }
}