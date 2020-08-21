using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metatool.NugetPackage;
using Metatool.Script.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Metatool.Script
{
    public class ScriptHost
    {
        private readonly ILogger _logger;
        private MetadataReference[] _defaultReferences;
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

        public async Task Build(string codePath, string outputDir, string assemblyName = null,
            OptimizationLevel optimization = OptimizationLevel.Debug, bool onlyBuild = true)
        {
            var code = File.ReadAllText(codePath);
            var codeDir = Path.GetDirectoryName(codePath);
            var refs = LibRefParser.ParseReference(code, codeDir);
            var id = assemblyName ?? Path.GetFileNameWithoutExtension(codePath);

            var stopWatch = Stopwatch.StartNew();

            var packageManager = new NugetManager(_logger) { Id = id, RestorePath = Path.Combine(outputDir, "nuget") };
            if (DefaultReferences.Length > 0)
            {
                refs.AddRange(GetReferencePaths(DefaultReferences).Select(p => new LibraryRef(p)));
            }
            _logger.LogInformation("Start to restore nuget packages...");
            var result = await packageManager.RestoreAsync(refs);

            if (result.Success)
            {
                _logger.LogInformation(
                     $"{id}: NugetPackage Restores successfully, time: {stopWatch.ElapsedMilliseconds}ms");

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
                    GetReferences(references: result.SuccessResult.CompileReferences);

                executionHostParameters.NuGetRuntimeReferences =
                    GetReferences(references: result.SuccessResult.RuntimeReferences);

                // reference directives & default references
                executionHostParameters.DirectReferences = packageManager.LocalLibraryPaths;

                var executionHost = new ExecutionHost(executionHostParameters, id, _logger);
                executionHost.Dumped += result => _logger.LogError(result.ToString());
                executionHost.Error += result => _logger.LogError(result.ToString());
                executionHost.ReadInput += () => _logger.LogInformation("read input");
                executionHost.NotifyBuildResult += e =>
                {
                    if (e.Count > 0)
                    {
                        _logger.LogError($"Build Error({id}): " + string.Join(Environment.NewLine, e));
                    }

                    NotifyBuildResult?.Invoke(e);
                };

                stopWatch.Restart();

                var res = await executionHost.BuildAndExecuteAsync(code, optimization, codePath, onlyBuild) ;
            }
            else
            {
                NotifyBuildResult?.Invoke(result.Errors.ToList().Select(er =>
                        CompilationErrorResultObject.Create("", "", "PackageRestoreError:: " + er, "", -1, -1)).ToList());
            };
        }

    }
}
