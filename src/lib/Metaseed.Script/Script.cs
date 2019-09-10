﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Metaseed.Metatool.Script.Resolver;
using Metaseed.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Metaseed.Metatool.Script
{
    public class Script
    {
        readonly Compiler _compiler = new Compiler();
        readonly Runner   _runner   = new Runner();

        public Script()
        {
            using var watcher = new ObservableFileSystemWatcher(c => { c.Path = @".\Sources"; });
            var changes = watcher.Changed.Throttle(TimeSpan.FromSeconds(.5))
                .Where(c => c.FullPath.EndsWith(@"DynamicProgram.cs")).Select(c => c.FullPath);

            //changes.Subscribe(filepath => _runner.Execute(_compiler.Compile(filepath), new[] {"France"}));

            watcher.Start();
        }

        void tt()
        {
            var code = File.ReadAllText("main.csx");
            var opts = ScriptOptions.Default.AddImports("System")
                .WithSourceResolver(new RemoteFileResolver());

            var script      = CSharpScript.Create(code, opts);
            var compilation = script.GetCompilation();
            var ilstream  = new MemoryStream();
            var pdbstream = new MemoryStream();
            compilation.Emit(ilstream, pdbstream);
            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any())
            {
                foreach (var diagnostic in diagnostics)
                {
                    Console.WriteLine(diagnostic.GetMessage());
                }
            }
            else
            {
                var result = script.RunAsync().Result;
            }
        }
    }
}