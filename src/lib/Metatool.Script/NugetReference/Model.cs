using System;
using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace Metatool.Script.NugetReference
{
    public class NuGetRestoreResult
    {
        public NuGetRestoreResult(IList<string> compileReferences, IList<string> runtimeReferences, IList<string> analyzers)
        {
            CompileReferences = compileReferences;
            RuntimeReferences = runtimeReferences;
            Analyzers         = analyzers;
        }

        public IList<string> CompileReferences { get; }
        public IList<string> RuntimeReferences { get; }
        public IList<string> Analyzers         { get; }
    }
    public class LibraryRef : IEquatable<LibraryRef?>
    {
        public LibraryRef(string id, VersionRange versionRange)
        {
            Id = id;
            VersionRange = versionRange;
        }

        public LibraryRef(string path) : this(string.Empty, VersionRange.AllFloating)
        {
            Path = path;
            AssemblyName = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public string Id { get; }
        public VersionRange VersionRange { get; }
        public string? Path { get; }
        public string? AssemblyName { get; }

        public bool Equals(LibraryRef? other)
        {
            return other != null &&
                (Id, VersionRange, Path).Equals((other.Id, other.VersionRange, other.Path));
        }

        public override bool Equals(object obj) => Equals(obj as LibraryRef);

        public override int GetHashCode()
        {
            return (Id, VersionRange).GetHashCode();
        }
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

        public IReadOnlyList<string> Errors  { get; }
        public bool                  Success { get; }
        public bool                  NoOp    { get; }
    }
}
