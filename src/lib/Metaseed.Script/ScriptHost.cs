using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Metaseed.Script.NugetReference;
using Metaseed.Script.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Metaseed.Script
{
    public class ScriptHost
    {
        private          ILogger                _logger;
        private          MetadataReference[]    _defaultReferences;
        private readonly ImmutableArray<string> _defaultImports;

        public ScriptHost(ILogger logger)
        {
            _logger = logger;
        }

        public MetadataReference[] DefaultReferences =>
            _defaultReferences ??= new[]
                {MetadataReference.CreateFromFile(typeof(RuntimeInitializer).Assembly.Location)};


        public ImmutableArray<string> DefaultImports = new[]
        {
            "System",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Collections",
            "System.Collections.Generic",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Linq",
            "System.IO",
            "System.Reflection",
            "Metaseed.Script.Runtime"
            // "System.Core",
            // "System.Data",
            // "System.Data.DataSetExtensions",
            // "System.Runtime",
            // "System.Xml",
            // "System.Xml.Linq",
            // "System.Net.Http",
            // "Microsoft.CSharp"
        }.ToImmutableArray();

        private IEnumerable<MetadataReference> _frameworkReferences;

        public ImmutableArray<MetadataReference> FrameworkReferences => (_frameworkReferences ??= new[]
        {
            typeof(Console).Assembly,
            typeof(int).Assembly,
            typeof(Regex).Assembly,
            typeof(System.Linq.Enumerable).Assembly
        }.Select(m =>
            MetadataReference.CreateFromFile(
                m.Location))).ToImmutableArray();


        public ImmutableArray<string> DisabledDiagnostics { get; } = ImmutableArray.Create("CS1701", "CS1702");

        IEnumerable<string> GetReferencePaths(IEnumerable<MetadataReference> references)
        {
            return references.OfType<PortableExecutableReference>().Select(x => x.FilePath);
        }

        ImmutableArray<string> GetReferences(IEnumerable<string> references)
        {
            return GetReferencePaths(DefaultReferences).Concat(references).ToImmutableArray();
        }

        public void Build(string path, string assemblyName = null, OptimizationLevel optimization= OptimizationLevel.Debug)
        {
            var code         = File.ReadAllText(path: path);
            var refs         = LibRefParser.ParseReference(code: code);
            var name         = assemblyName?? Path.GetFileNameWithoutExtension(path: path);
            var directory    = Path.GetDirectoryName(path: path);
            var nugetPackage = new NugetPackage(logger: _logger);

            var packageViewModel = new PackageViewModel(logger: _logger, nugetPackage: nugetPackage)
                {Id = name, RestorePath = Path.Combine(path1: directory, path2: "nuget")};


            List<CompilationErrorResultObject> ResultsInternal = new List<CompilationErrorResultObject>();
            nugetPackage.RestoreCompleted += async restoreResult =>
            {
                var executionHostParameters = new ExecutionHostParameters(
                    compileReferences: ImmutableArray<string>.Empty, 
                    runtimeReferences: ImmutableArray<string>.Empty,
                    directReferences: ImmutableArray<string>.Empty,
                    frameworkReferences: FrameworkReferences,
                    imports: DefaultImports,
                    disabledDiagnostics: DisabledDiagnostics,
                    workingDirectory: directory,
                    globalPackageFolder: nugetPackage.GlobalPackageFolder);
                executionHostParameters.NuGetCompileReferences =
                    GetReferences(references: restoreResult.CompileReferences);
                // runtime references from NuGet
                executionHostParameters.NuGetRuntimeReferences =
                    GetReferences(references: restoreResult.RuntimeReferences);

                // reference directives & default references
                executionHostParameters.DirectReferences = packageViewModel.LocalLibraryPaths;

                var executionHost =
                    new ExecutionHost(parameters: executionHostParameters, buildPath: directory, name: name);
                // executionHost.Dumped            += AddResult;
                // executionHost.Error             += ExecutionHostOnError;
                // executionHost.ReadInput         += ExecutionHostOnInputRequest;
                // executionHost.CompilationErrors += ExecutionHostOnCompilationErrors;
                executionHost.NotifyBuildResult += errors =>
                {
                    if (errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            ResultsInternal.Add(error);
                        }

                        _logger.LogError(string.Join(Environment.NewLine, errors));
                        // ResultsAvailable?.Invoke();
                    }
                    else
                    {
                        _logger.LogInformation($"Assembly {name}: build successfully!");
                    }
                };
                await executionHost.BuildAndExecuteAsync(code: code, optimizationLevel: optimization);
            };
            if (DefaultReferences.Length > 0)
            {
              
                refs.AddRange(GetReferencePaths(DefaultReferences).Select(p => new LibraryRef(p)));
            }
            packageViewModel.UpdateLibraries(libraries: refs);
        }

        // private void AddResult(IResultObject o)
        // {
        //     _dispatcher.InvokeAsync(() =>
        //     {
        //         ResultsInternal?.Add(o);
        //         ResultsAvailable?.Invoke();
        //     }, AppDispatcherPriority.Low);
        // }
    }
}