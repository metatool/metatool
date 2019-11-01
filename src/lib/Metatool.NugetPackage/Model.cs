using System;
using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Metatool.NugetPackage
{
    public class RestoreSuccessResult
    {
        public RestoreSuccessResult(IList<string> compileReferences, IList<string> runtimeReferences, IList<string> analyzers)
        {
            CompileReferences = compileReferences;
            RuntimeReferences = runtimeReferences;
            Analyzers         = analyzers;
        }

        public IList<string> CompileReferences { get; }
        public IList<string> RuntimeReferences { get; }
        public IList<string> Analyzers         { get; }
    }

    public class RestoreParams
    {
        public string ProjectName { get; set; }
        public NuGetFramework TargetFramework { get; set; }
        public string? FrameworkVersion { get; set; }
        public string OutputPath { get; set; }
        public string PackagesPath { get; set; }
        public IList<string> ConfigFilePaths { get; set; } = new List<string>();
        public IList<PackageSource> Sources { get; set; } = new List<PackageSource>();
        public IList<LibraryRef>? Libraries { get; set; } = new List<LibraryRef>();
    }
    public class RestoreResult
    {
        public RestoreResult(IReadOnlyList<string> errors, bool success, bool noOp)
        {
            Errors  = errors;
            Success = success;
            NoOp    = noOp;
        }

        public RestoreSuccessResult SuccessResult { get; set; }
        public IReadOnlyList<string> Errors  { get; }
        public bool                  Success { get; }
        public bool                  NoOp    { get; }
    }
}
