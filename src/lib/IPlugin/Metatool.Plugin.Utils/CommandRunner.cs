using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;

namespace Metatool.Utils
{
    public class CommandRunner
    {
        private static readonly ILogger _logger = Services.Get<ILogger<CommandRunner>>();


        public static int Run(string commandPath, string arguments = null, string workingDirectory = null)
        {
            _logger.LogDebug($"Executing '{commandPath} {arguments}'");
            var startInformation = CreateProcessStartInfo(commandPath, arguments, workingDirectory);
            var process          = CreateProcess(startInformation);
            RunAndWait(process);
            return process.ExitCode;
        }

        public static CommandResult Capture(string commandPath, string arguments, string workingDirectory = null)
        {
            var startInformation = CreateProcessStartInfo(commandPath, arguments, workingDirectory);
            var process          = CreateProcess(startInformation);
            process.Start();
            var standardOut   = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new CommandResult(process.ExitCode, standardOut, standardError);
        }


        // Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
        // so we use cmd to make a workaround
        // https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/cmd
        // https: //ss64.com/nt/cmd.html
        public static void RunWithCmd(string cmd, params string[] args )
        {
            var proc = new Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName               = "cmd",
                    Arguments              = "/c " + BuildCmd(cmd, args),
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    WorkingDirectory = Context.AppDirectory
                }
            };
            proc.Start();
        }

        /// <summary>
        /// Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
        /// so we use cmd to make a workaround
        ///
        /// this could run *.lnk and *.bat
        /// </summary>
        /// <param name="filePath"></param>
        public static void RunWithExplorer(string filePath, string workingDir = null)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName        = "explorer.exe",
                    ArgumentList = { filePath },
                    CreateNoWindow  = true,
                    UseShellExecute = false,
                    WindowStyle     = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingDir??Context.AppDirectory
                }
            };
            proc.Start();
        }

        private static string BuildCmd(string cmd, params string[] args)
        {
            var a = string.Join(" ", args.ToList().Select(arg => arg.Any(char.IsWhiteSpace) ? $"\"{arg}\"" : arg));
            return $"\"\"{cmd}\" {a}\"";
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

    public class CommandResult
    {
        public CommandResult(int exitCode, string standardOut, string standardError)
        {
            ExitCode      = exitCode;
            StandardOut   = standardOut;
            StandardError = standardError;
        }

        public string StandardOut   { get; }
        public string StandardError { get; }
        public int    ExitCode      { get; }

        public CommandResult EnsureSuccessfulExitCode(int success = 0)
        {
            if (ExitCode != success)
            {
                throw new InvalidOperationException(StandardError);
            }

            return this;
        }
    }
}