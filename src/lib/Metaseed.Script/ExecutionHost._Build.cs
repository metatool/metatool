using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metaseed.Metatool.Script.Resolver;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoslynPad.Hosting;

namespace Metaseed.Script
{
    public partial class ExecutionHost
    {
        private static readonly CSharpParseOptions _parseOptions = new CSharpParseOptions(
            preprocessorSymbols: new[] {"__DEMO__", "__DEMO_EXPERIMENTAL__"}, languageVersion: LanguageVersion.Latest,
            kind: SourceCodeKind.Script);

        private static readonly SyntaxTree InitHostSyntax = SyntaxFactory.ParseSyntaxTree(
            @"Metaseed.Script.Runtime.RuntimeInitializer.Initialize();", _parseOptions);

        private ExecutionHostParameters                          _parameters;
        private ScriptOptions                                    _scriptOptions;
        private bool                                             _running;
        private string                                           _assemblyPath;
        private string                                           _depsFile;
        private CancellationTokenSource                          _executeCts;
        private bool                                             _initializeBuildPathAfterRun;
        public event Action<IList<CompilationErrorResultObject>> CompilationErrors;


        public ExecutionHost(ExecutionHostParameters parameters, string buildPath, string name)
        {
            _parameters = parameters;
            BuildPath   = buildPath;
            Name        = name;

            Initialize(parameters);
            // for execu
            _jsonSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public Task Update(ExecutionHostParameters parameters)
        {
            Initialize(parameters);
            return Task.CompletedTask;
        }

        public string Name { get; set; }

        public async Task BuildAndExecuteAsync(string code, OptimizationLevel? optimizationLevel, bool onlyBuild = true)
        {
            await new NoContextYieldAwaitable();

            try
            {
                _running = true;

                using var executeCts        = new CancellationTokenSource();
                var       cancellationToken = executeCts.Token;

                var script = CreateScriptRunner(code, optimizationLevel);

                _assemblyPath = Path.Combine(BuildPath, $"{Name}.dll");
                _depsFile     = Path.ChangeExtension(_assemblyPath, ".deps.json");

                CopyDependencies();

                var diagnostics = await script.SaveAssembly(_assemblyPath, cancellationToken).ConfigureAwait(false);
                SendDiagnostics(diagnostics);

                if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return;
                }

                _executeCts = executeCts;

                if (!onlyBuild)
                    await RunProcess(_assemblyPath, cancellationToken);
            }
            finally
            {
                _executeCts = null;
                _running    = false;

                if (_initializeBuildPathAfterRun)
                {
                    _initializeBuildPathAfterRun = false;
                    InitializeBuildPath(stop: false);
                }
            }
        }

        private void InitializeBuildPath(bool stop)
        {
            if (stop)
            {
                StopProcess();
            }
            else if (_running)
            {
                _initializeBuildPathAfterRun = true;
                return;
            }

            CleanupBuildPath();
            CreateRuntimeConfig();
        }

        private void CleanupBuildPath()
        {
            StopProcess();
            try
            {
                foreach (var file in Directory.EnumerateFiles(BuildPath))
                {
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CreateRuntimeConfig()
        {
            var config = DotNetConfigHelper.CreateNetCoreRuntimeOptions();
            WriteJson(Path.Combine(BuildPath, $"{Name}.runtimeconfig.json"), config);

            var devConfig = DotNetConfigHelper.CreateNetCoreDevRuntimeOptions(_parameters.GlobalPackageFolder);
            WriteJson(Path.Combine(BuildPath, $"{Name}.runtimeconfig.dev.json"), devConfig);
        }

        private static void WriteJson(string path, JToken token)
        {
            using (var file = File.CreateText(path))
            using (var writer = new JsonTextWriter(file))
            {
                token.WriteTo(writer);
            }
        }

        private void StopProcess()
        {
            _executeCts?.Cancel();
        }

        private void SendDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            if (diagnostics.Length > 0)
            {
                CompilationErrors?.Invoke(diagnostics.Where(d => !_parameters.DisabledDiagnostics.Contains(d.Id))
                    .Select(GetCompilationErrorResultObject).ToImmutableArray());
            }
        }

        private static CompilationErrorResultObject GetCompilationErrorResultObject(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetLineSpan();

            var result = CompilationErrorResultObject.Create(diagnostic.Severity.ToString(),
                diagnostic.Id, diagnostic.GetMessage(),
                lineSpan.StartLinePosition.Line, lineSpan.StartLinePosition.Character);
            return result;
        }

        private void Initialize(ExecutionHostParameters parameters)
        {
            _parameters = parameters;
            _scriptOptions = ScriptOptions.Default
                .WithReferences(parameters.NuGetRuntimeReferences.Select(p => MetadataReference.CreateFromFile(p))
                    .Concat(parameters.FrameworkReferences))
                .WithImports(parameters.Imports)
                .WithMetadataResolver(new NuGetMetadataReferenceResolver(parameters.WorkingDirectory))
                .WithSourceResolver(new RemoteFileResolver(AppContext.BaseDirectory));
        }

        private ScriptRunner CreateScriptRunner(string code, OptimizationLevel? optimizationLevel)
        {
            return new ScriptRunner(code:null,
                syntaxTrees: ImmutableList.Create(InitHostSyntax, ParseCode(code: code)),
                parseOptions: _parseOptions,
                outputKind: OutputKind.ConsoleApplication,
                platform: Platform.AnyCpu,
                references: _scriptOptions.MetadataReferences,
                usings: _scriptOptions.Imports,
                filePath: _scriptOptions.FilePath,
                workingDirectory: _parameters.WorkingDirectory,
                metadataResolver: _scriptOptions.MetadataResolver,
                sourceResolver: _scriptOptions.SourceResolver,
                optimizationLevel: optimizationLevel ?? _parameters.OptimizationLevel,
                checkOverflow: _parameters.CheckOverflow,
                allowUnsafe: _parameters.AllowUnsafe);
        }

        private SyntaxTree ParseCode(string code)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(code, _parseOptions);
            var root = tree.GetRoot();

            if (root is CompilationUnitSyntax c)
            {
                var members = c.Members;

                // add .Dump() to the last bare expression
                var lastMissingSemicolon = c.Members.OfType<GlobalStatementSyntax>()
                    .LastOrDefault(m => m.Statement is ExpressionStatementSyntax expr && expr.SemicolonToken.IsMissing);
                if (lastMissingSemicolon != null)
                {
                    var statement = (ExpressionStatementSyntax) lastMissingSemicolon.Statement;

                    members = members.Replace(lastMissingSemicolon,
                        SyntaxFactory.GlobalStatement(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        statement.Expression,
                                        SyntaxFactory.IdentifierName(nameof(ObjectExtensions.Dump)))))));
                }

                root = c.WithMembers(members);
            }

            return tree.WithRootAndOptions(root, _parseOptions);
        }

        private void CopyDependencies()
        {
            var referencesChanged = CopyReferences(_parameters.DirectReferences);


            FileCopy(Path.Combine(BuildPath, "nuget", "project.assets.json"), _depsFile, overwrite: true);

            bool CopyReferences(IEnumerable<string> references)
            {
                var copied = false;

                foreach (var file in references)
                {
                    if (CopyIfNewer(file, Path.Combine(BuildPath, Path.GetFileName(file))))
                    {
                        copied = true;
                    }
                }

                return copied;
            }
        }

        public string BuildPath { get; set; }

        private static bool CopyIfNewer(string source, string destination)
        {
            var sourceInfo      = new FileInfo(source);
            var destinationInfo = new FileInfo(destination);

            if (!destinationInfo.Exists || destinationInfo.CreationTimeUtc < sourceInfo.CreationTimeUtc)
            {
                FileCopy(source, destination, overwrite: true);
                return true;
            }

            return false;
        }

        private static void FileCopy(string source, string destination, bool overwrite)
        {
            const int ERROR_ENCRYPTION_FAILED = unchecked((int) 0x80071770);

            try
            {
                File.Copy(source, destination, overwrite);
            }
            catch (IOException ex) when (ex.HResult == ERROR_ENCRYPTION_FAILED)
            {
                using (var read = File.OpenRead(source))
                using (var write = new FileStream(destination, overwrite ? FileMode.Create : FileMode.CreateNew))
                {
                    read.CopyTo(write);
                }
            }
        }
    }
}