using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Metaseed.Metatool.Script.Resolver;
using Metaseed.Script.NugetReference;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Metaseed.Script
{
    static class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "cc.csx");

            var code = File.ReadAllText(path);
            var ss =CSharpScript.Create(code);
            var s = LibRefParser.ParseReference(code);
            var p = new NugetPackage(null);
           
            p.RestoreCompleted += a =>
            {
                IEnumerable<string> GetReferencePaths(IEnumerable<MetadataReference> references)
                {
                    return references.OfType<PortableExecutableReference>().Select(x => x.FilePath);
                }

                // ImmutableArray<string> GetReferences(IEnumerable<string> references, Roslyn.RoslynHost host)
                // {
                //     return GetReferencePaths(host.DefaultReferences).Concat(references).ToImmutableArray();
                // }
                // var _executionHostParameters = new ExecutionHostParameters(
                //     ImmutableArray<string>.Empty, // will be updated during NuGet restore
                //     ImmutableArray<string>.Empty,
                //     ImmutableArray<string>.Empty,
                //     ImmutableArray<MetadataReference>.Empty,
                //     roslynHost.DefaultImports,
                //     roslynHost.DisabledDiagnostics,
                //     WorkingDirectory,
                //     MainViewModel.NuGet.GlobalPackageFolder);
                // _executionHostParameters.FrameworkReferences = useDesktopReferences ? MainViewModel.DesktopReferences : ImmutableArray<MetadataReference>.Empty;
                // // compile-time references from NuGet
                // _executionHostParameters.NuGetCompileReferences = GetReferences(restoreResult.CompileReferences, host);
                // // runtime references from NuGet
                // _executionHostParameters.NuGetRuntimeReferences = GetReferences(restoreResult.RuntimeReferences, host);
                // // reference directives & default references
                // _executionHostParameters.DirectReferences = NuGet.LocalLibraryPaths;
                // _executionHostParameters.NuGetCompileReferences.Select(p => MetadataReference.CreateFromFile(p)).Concat(parameters.FrameworkReferences)
                var o = ScriptOptions.Default.WithReferences(a.RuntimeReferences.Select(ap => MetadataReference.CreateFromFile(ap)).Concat(new []{typeof(Console).Assembly, typeof(int).Assembly, typeof(Regex).Assembly, typeof(System.Linq.Enumerable).Assembly}.Select(m=>MetadataReference.CreateFromFile(m.Location)))).WithImports(new[]{
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
                    // "System.Core",
                    // "System.Data",
                    // "System.Data.DataSetExtensions",
                    // "System.Runtime",
                    // "System.Xml",
                    // "System.Xml.Linq",
                    // "System.Net.Http",
                    // "Microsoft.CSharp"
                });
                var runner = new ScriptRunner(code, references: o.MetadataReferences, usings: o.Imports, workingDirectory: AppContext.BaseDirectory, sourceResolver: new RemoteFileResolver(AppContext.BaseDirectory), metadataResolver: new NuGetMetadataReferenceResolver(AppContext.BaseDirectory)
                );

                var aaaaaaa = runner.SaveAssembly(Path.Combine(AppContext.BaseDirectory, "cc", "cc.dll")).GetAwaiter().GetResult();
            };
            var m =new PackageViewModel(null,p){Id = "cc"};
            var g = Path.Combine(AppContext.BaseDirectory, "cc", "nuget");
            m.BuildPath = g;
            m.UpdateLibraries(s);


            Console.ReadLine();
            //var bbbss =runner.RunAsync().GetAwaiter().GetResult();
        }
    }
}
