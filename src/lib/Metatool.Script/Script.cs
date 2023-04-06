using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Metatool.Metatool.Script.Resolver;
using Metatool.Reactive;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Metatool.Metatool.Script;

public class Script
{
	readonly Compiler _compiler = new();
	readonly Runner   _runner   = new();

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