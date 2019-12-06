using System;
using System.Collections.Generic;
using System.Text;

namespace Metatool.Service
{
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
    public interface IShell
    {
        int Run(string commandPath, string arguments = null, string workingDirectory = null);
        CommandResult Capture(string commandPath, string arguments, string workingDirectory = null);
        string NormalizeCmd(params string[] cmdArgsSerials);
        void RunWithCmd(string cmdWithArgs, bool asAdmin = false, string workingDir = null);
        void RunWithExplorer(string filePath, string workingDir = null);
        void RunAsNormalUser(string cmd, params string[] args);

        void CreateShortcut(string targetPath, string shortcutPath, string hotkey = "",
            string description = "", bool isAdmin = false);
    }
}
