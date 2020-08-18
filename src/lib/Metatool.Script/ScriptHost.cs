using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Metatool.NugetPackage;
using Metatool.Script.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace Metatool.Script
{
    public class ScriptHost
    {
        private ILogger _logger;
        private MetadataReference[] _defaultReferences;
        private readonly ImmutableArray<string> _defaultImports;
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

        public void Build(string codePath, string outputDir, string assemblyName = null,
            OptimizationLevel optimization = OptimizationLevel.Debug)
        {
            var code = File.ReadAllText(codePath);
            var codeDir = Path.GetDirectoryName(codePath);
            var refs = LibRefParser.ParseReference(code, codeDir);
            var id = assemblyName ?? Path.GetFileNameWithoutExtension(codePath);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var packageManager = new NugetManager(_logger) { Id = id, RestorePath = Path.Combine(outputDir, "nuget") };

            packageManager.RestoreSuccess += async restoreResult =>
            {
                _logger.LogInformation(
                    $"{assemblyName}: NugetPackage Restores successfully, time: {stopWatch.ElapsedMilliseconds}ms");

                var executionHostParameters = new ExecutionHostParameters(
                    compileReferences: ImmutableArray<string>.Empty,
                    runtimeReferences: ImmutableArray<string>.Empty,
                    directReferences: ImmutableArray<string>.Empty,
                    frameworkReferences: FrameworkReferences,
                    imports: DefaultImports,
                    disabledDiagnostics: DisabledDiagnostics,
                    outputDirectory: outputDir,
                    workingDirectory: codeDir,
                    globalPackageFolder: packageManager.PackageFolder);

                executionHostParameters.NuGetCompileReferences =
                    GetReferences(references: restoreResult.CompileReferences);

                executionHostParameters.NuGetRuntimeReferences =
                    GetReferences(references: restoreResult.RuntimeReferences);

                // reference directives & default references
                executionHostParameters.DirectReferences = packageManager.LocalLibraryPaths;

                var executionHost =
                    new ExecutionHost(executionHostParameters, id, _logger);
                // executionHost.Dumped            += AddResult;
                // executionHost.Error             += ExecutionHostOnError;
                // executionHost.ReadInput         += ExecutionHostOnInputRequest;
                // executionHost.CompilationErrors += ExecutionHostOnCompilationErrors;
                executionHost.NotifyBuildResult += e => NotifyBuildResult?.Invoke(e);

                stopWatch.Restart();

                _logger.LogInformation($"{assemblyName}: Start to build...");
                var result = await executionHost.BuildAndExecuteAsync(code, optimization, codePath) ? "successfully" : "error";
                _logger.LogInformation($"{assemblyName}: Build {result} , time: {stopWatch.ElapsedMilliseconds}ms");
            };
            if (DefaultReferences.Length > 0)
            {
                refs.AddRange(GetReferencePaths(DefaultReferences).Select(p => new LibraryRef(p)));
            }

            packageManager.RestoreError += r =>
            {
                NotifyBuildResult?.Invoke(r.ToList().Select(er =>
                        CompilationErrorResultObject.Create("", "", "PackageRestoreError:: " + er, "", -1, -1))
                    .ToList());
            };
            packageManager.Restore(refs);
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
