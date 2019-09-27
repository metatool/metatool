using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Metatool.Script.NugetReference;
using Metatool.Script.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Metatool.Script
{
    public class ScriptHost
    {
        private          ILogger                                 _logger;
        private          MetadataReference[]                     _defaultReferences;
        private readonly ImmutableArray<string>                  _defaultImports;
        public event Action<IList<CompilationErrorResultObject>> NotifyBuildResult;

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
            "Metatool.Script.Runtime"
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

        public void Build(string path, string assemblyName = null,
            OptimizationLevel optimization = OptimizationLevel.Debug)
        {
            var code         = File.ReadAllText( path);
            var refs         = LibRefParser.ParseReference(code);
            var name         = assemblyName ?? Path.GetFileNameWithoutExtension( path);
            var directory    = Path.GetDirectoryName(path);
            var nugetPackage = new NugetPackage(_logger);

            var packageViewModel = new PackageViewModel(_logger, nugetPackage)
                {Id = name, RestorePath = Path.Combine(directory, "nuget")};


            nugetPackage.RestoreResult += async restoreResult =>
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
                    new ExecutionHost(executionHostParameters,  directory, name);
                // executionHost.Dumped            += AddResult;
                // executionHost.Error             += ExecutionHostOnError;
                // executionHost.ReadInput         += ExecutionHostOnInputRequest;
                // executionHost.CompilationErrors += ExecutionHostOnCompilationErrors;
                executionHost.NotifyBuildResult += e => NotifyBuildResult?.Invoke(e);
                await executionHost.BuildAndExecuteAsync(code, optimization);
            };
            if (DefaultReferences.Length > 0)
            {
                refs.AddRange(GetReferencePaths(DefaultReferences).Select(p => new LibraryRef(p)));
            }

            packageViewModel.RestoreError += r => { NotifyBuildResult?.Invoke(r.ToList().Select(er=>CompilationErrorResultObject.Create("","","PackageRestoreError:: "+er, "", -1,-1)).ToList()); };
            packageViewModel.UpdateLibraries(refs);

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
