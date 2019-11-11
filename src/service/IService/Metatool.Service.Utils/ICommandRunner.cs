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
    public interface ICommandRunner
    {
        int Run(string commandPath, string arguments = null, string workingDirectory = null);
        CommandResult Capture(string commandPath, string arguments, string workingDirectory = null);
        void RunWithCmd(string cmd, params string[] args);
        void RunWithExplorer(string filePath, string workingDir = null);
        void RunAsNormalUser(string exeWithArgs);
    }
}
