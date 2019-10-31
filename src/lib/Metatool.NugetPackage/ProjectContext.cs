using System;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace Metatool.NugetPackage
{
    public class ProjectContext : INuGetProjectContext
    {
        private ILogger _logger = NugetHelper.Instance.Logger;
        public void Log(MessageLevel level, string message, params object[] args)
        {
            _logger.Log((LogLevel)level, $"{message}, args: {string.Join(",", args)}");
        }

        public FileConflictAction ResolveFileConflict(string message) => FileConflictAction.Ignore;

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public XDocument OriginalPackagesConfig { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider => null;

        public ExecutionContext ExecutionContext => null;

        public void ReportError(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(ILogMessage message)
        {
            _logger.Log(message);
        }

        public void ReportError(ILogMessage message)
        {
            _logger.Log(message);
        }

        public NuGetActionType ActionType { get; set; }
        public Guid OperationId { get; set; }
    }
}