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
            var path = Path.Combine(AppContext.BaseDirectory,"BBB", "cc.csx");

            new ScriptHost(null).Build(path, OptimizationLevel.Debug);
            //var code = File.ReadAllText(path);
            //var s = LibRefParser.ParseReference(code);
            //var p = new NugetPackage(null);

            //p.RestoreCompleted += a =>
            //{
            //    IEnumerable<string> GetReferencePaths(IEnumerable<MetadataReference> references)
            //    {
            //        return references.OfType<PortableExecutableReference>().Select(x => x.FilePath);
            //    }

            //    var o = ScriptOptions.Default.WithReferences(a.RuntimeReferences.Select(ap => MetadataReference.CreateFromFile(ap)).Concat(new[] { typeof(Console).Assembly, typeof(int).Assembly, typeof(Regex).Assembly, typeof(System.Linq.Enumerable).Assembly }.Select(m => MetadataReference.CreateFromFile(m.Location)))).WithImports(new[]{
            //        "System",
            //        "System.Threading",
            //        "System.Threading.Tasks",
            //        "System.Collections",
            //        "System.Collections.Generic",
            //        "System.Text",
            //        "System.Text.RegularExpressions",
            //        "System.Linq",
            //        "System.IO",
            //        "System.Reflection",
            //        // "System.Core",
            //        // "System.Data",
            //        // "System.Data.DataSetExtensions",
            //        // "System.Runtime",
            //        // "System.Xml",
            //        // "System.Xml.Linq",
            //        // "System.Net.Http",
            //        // "Microsoft.CSharp"
            //    });
            //    var runner = new ScriptRunner(code, references: o.MetadataReferences, usings: o.Imports, workingDirectory: AppContext.BaseDirectory, sourceResolver: new RemoteFileResolver(AppContext.BaseDirectory), metadataResolver: new NuGetMetadataReferenceResolver(AppContext.BaseDirectory)
            //    );

            //    var aaaaaaa = runner.SaveAssembly(Path.Combine(AppContext.BaseDirectory, "cc", "cc.dll")).GetAwaiter().GetResult();
            //};
            //var m = new PackageViewModel(null, p) { Id = "cc" };
            //var g = Path.Combine(AppContext.BaseDirectory, "cc", "nuget");
            //m.RestorePath = g;
            //m.UpdateLibraries(s);


            Console.ReadLine();
            //var bbbss =runner.RunAsync().GetAwaiter().GetResult();
        }
    }
}
