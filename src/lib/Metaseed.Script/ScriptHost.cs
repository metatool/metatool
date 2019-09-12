using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Metaseed.Script.NugetReference;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Metaseed.Script
{
    public class ScriptHost
    {
        private ILogger _logger;

        public ScriptHost(ILogger logger)
        {
            _logger = logger;
        }
        public ImmutableArray<MetadataReference> DefaultReferences { get;22 }

        public ImmutableArray<string> DefaultImports { get;22 }
        public ImmutableArray<string> DisabledDiagnostics { get; } = ImmutableArray.Create("CS1701", "CS1702");

        IEnumerable<string> GetReferencePaths(IEnumerable<MetadataReference> references)
        {
            return references.OfType<PortableExecutableReference>().Select(x => x.FilePath);
        }

        ImmutableArray<string> GetReferences(IEnumerable<string> references)
        {
            return GetReferencePaths(DefaultReferences).Concat(references).ToImmutableArray();
        }
        public bool Build(string path)
        {
            var code = File.ReadAllText(path);
            var refs = LibRefParser.ParseReference(code);
            var name = Path.GetFileName(path);
            var directory  = Path.GetDirectoryName(path);

            var nugetPackage = new NugetPackage(_logger);
            var packageViewModel = new PackageViewModel(_logger, nugetPackage) { Id = name, RestorePath = Path.Combine(directory, "nuget") };

            nugetPackage.RestoreCompleted += restoreResult =>
            {
            
                var _executionHostParameters = new ExecutionHostParameters(
                    ImmutableArray<string>.Empty, // will be updated during NuGet restore
                    ImmutableArray<string>.Empty,
                    ImmutableArray<string>.Empty,
                    ImmutableArray<MetadataReference>.Empty,
                    DefaultImports,
                    DisabledDiagnostics,
                    directory,
                    nugetPackage.GlobalPackageFolder);
                _executionHostParameters.NuGetCompileReferences = GetReferences(restoreResult.CompileReferences);
                // runtime references from NuGet
                _executionHostParameters.NuGetRuntimeReferences = GetReferences(restoreResult.RuntimeReferences);
                // reference directives & default references
                _executionHostParameters.DirectReferences = packageViewModel.LocalLibraryPaths;

                var _executionHost = new ExecutionHost(directory,name);
                var task = _executionHost?.Update(_executionHostParameters);
            };
               
            packageViewModel.UpdateLibraries(refs);
        }
    }
}