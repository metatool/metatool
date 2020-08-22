using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metatool.Metatool.Script.Resolver;
using Metatool.Service;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Metatool.Script
{
    public partial class ExecutionHost
    {
        private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(
            preprocessorSymbols: new[]
            {
                "__DEMO__", "__DEMO_EXPERIMENTAL__"
            },
            languageVersion: LanguageVersion.Latest,
            kind: SourceCodeKind.Script
        );

        private static readonly SyntaxTree InitHostSyntax = SyntaxFactory.ParseSyntaxTree(
            @"Metatool.Script.Runtime.RuntimeInitializer.Initialize(); var Args = Metatool.Script.Runtime.RuntimeInitializer.Args;", ParseOptions);

        private ExecutionHostParameters _parameters;
        private ScriptOptions _scriptOptions;
        private string _assemblyPath;
        private string _depsFile;
        private ILogger _logger;
        public event Action<IList<CompilationErrorResultObject>> NotifyBuildResult;

        public ExecutionHost(ExecutionHostParameters parameters, string name, ILogger logger)
        {
            _logger = logger;
            _parameters = parameters;
            Name = name;

            Initialize(parameters);
            // for executing
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

        public async Task<bool> BuildAsync(string code, OptimizationLevel? optimizationLevel, string codePath,
            CancellationToken cancellationToken = default)
        {
            await new NoContextYieldAwaitable();
            _logger.LogInformation($"{Name}: Start to build...");
            var sw = Stopwatch.StartNew();
            try
            {
                _assemblyPath = Path.Combine(BuildPath, $"{Name}.dll");
                _depsFile = Path.ChangeExtension(_assemblyPath, ".deps.json");

                var encoding = Encoding.UTF8;
                var buffer = encoding.GetBytes(code);
                var sourceText = SourceText.From(buffer, buffer.Length, encoding, canBeEmbedded: true);

                var scriptRunner = CreateScriptRunner(sourceText, codePath, optimizationLevel);

                CopyDependencies();

                var embeddedTexts = new List<EmbeddedText>()
                {
                    EmbeddedText.FromSource(codePath, sourceText),
                };

                var diagnostics = await scriptRunner.SaveAssembly(_assemblyPath, embeddedTexts, cancellationToken)
                    .ConfigureAwait(false);
                SendDiagnostics(diagnostics);

                var error = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
                _logger.LogInformation($"{Name}: Build {(error ?  "error": "successfully")} , time: {sw.ElapsedMilliseconds}ms");
                if (error)
                {
                    return false;
                }
                CreateRuntimeConfig();
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message + e.StackTrace);
                return false;
            }

            return true;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default) => await RunProcess(_assemblyPath, cancellationToken);

        private void CleanupBuildPath()
        {
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

        private void SendDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            NotifyBuildResult?.Invoke(diagnostics.Where(d => !_parameters.DisabledDiagnostics.Contains(d.Id))
                .Select(GetCompilationErrorResultObject).ToImmutableArray());
        }

        private static CompilationErrorResultObject GetCompilationErrorResultObject(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.GetLineSpan();
            var result = CompilationErrorResultObject.Create(diagnostic.Severity.ToString(),
                diagnostic.Id, diagnostic.GetMessage(), lineSpan.Path,
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
                .WithMetadataResolver(new NuGetMetadataReferenceResolver(parameters.OutputDirectory))
                .WithSourceResolver(new RemoteFileResolver(parameters.WorkingDirectory));
        }

        private ScriptRunner CreateScriptRunner(SourceText sourceText, string codePath,
            OptimizationLevel? optimizationLevel)
        {
            return new ScriptRunner(code: null,
                syntaxTrees: ImmutableList.Create(InitHostSyntax, ParseCode(sourceText, codePath)),
                parseOptions: ParseOptions,
                outputKind: OutputKind.ConsoleApplication,
                platform: Platform.AnyCpu,
                references: _scriptOptions.MetadataReferences,
                usings: _scriptOptions.Imports,
                filePath: _scriptOptions.FilePath,
                workingDirectory: _parameters.OutputDirectory,
                metadataResolver: _scriptOptions.MetadataResolver,
                sourceResolver: _scriptOptions.SourceResolver,
                optimizationLevel: optimizationLevel ?? _parameters.OptimizationLevel,
                checkOverflow: _parameters.CheckOverflow,
                allowUnsafe: _parameters.AllowUnsafe);
        }

        private SyntaxTree ParseCode(SourceText sourceText, string codePath)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceText, ParseOptions, codePath);
            var root = tree.GetRoot();

            if (root is CompilationUnitSyntax c)
            {
                var members = c.Members;

                // add .Dump() to the last bare expression
                var lastMissingSemicolon = c.Members.OfType<GlobalStatementSyntax>()
                    .LastOrDefault(m => m.Statement is ExpressionStatementSyntax expr && expr.SemicolonToken.IsMissing);
                if (lastMissingSemicolon != null)
                {
                    var statement = (ExpressionStatementSyntax)lastMissingSemicolon.Statement;

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

            var rootNode = root as CSharpSyntaxNode;
            return CSharpSyntaxTree.Create(rootNode, ParseOptions, codePath, sourceText.Encoding);
            // return tree.WithRootAndOptions(root, _parseOptions);
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

        public string BuildPath => _parameters.OutputDirectory;

        private static bool CopyIfNewer(string source, string destination)
        {
            var sourceInfo = new FileInfo(source);
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
            const int ERROR_ENCRYPTION_FAILED = unchecked((int)0x80071770);

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