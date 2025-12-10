using System;
using System.IO;
using System.CommandLine;
using Metatool.Plugin;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Metaseed.Metatool;

// https://github.com/dotnet/command-line-api
public class ArgumentProcessor
{
	private readonly ILogger _logger;
	private readonly string[] _args;

	public ArgumentProcessor(string[] args)
	{
		_args = args;

		if (args.Length == 0 || args[0] == "run")
			ConsoleExt.InitialConsole(true, Context.IsElevated);
		_logger = Services.Get<ILogger<ArgumentProcessor>>();
	}

	const string HelpOptionTemplate = "-? | -h | --help";

	public Task<int> Run()
	{
		var app = new RootCommand()
		{
			Name = "metatool",
			Description = "tools for Windows",
		};

		//app.HelpOption(inherited: true);

		app.SetHandler(() =>
		{
			// without sub command
			App.RunApp();
			Services.GetOrCreate<PluginManager>().InitPlugins();
		});

		var newCmd = new Command("new");
		app.Add(newCmd);

		newCmd.AddAlias("n");
		newCmd.SetHandler(() =>
		{
			Console.WriteLine("Please specify a subcommand");
			//configCmd.ShowHelp();
			//return 1;
		});

		var newScriptCmd = new Command("script", "Creates a sample script tool along with the files needed to launch and debug the script.");
		newCmd.Add(newScriptCmd);

		var nameArg = new Argument<string>("name",
			"The name of the tool script to be created.");
		newScriptCmd.AddArgument(nameArg);

		var dirOption = new Option<string>(new[] { "--directory", "-dir" }, () =>
		{
			return Directory.GetCurrentDirectory();
		}, "The directory to initialize the tool scripts. Defaults to current directory.");

		newScriptCmd.SetHandler((string name, string directory) =>
		{
			if (string.IsNullOrEmpty(name))
			{
				Console.WriteLine("please set the tool name \nusage: metatool new script <name>");
				return;
			}

			if (!Directory.Exists(directory))
			{
				Console.WriteLine("the directory is not exist");
				return;
			}

			var scaffolder = new Scaffold(_logger);
			scaffolder.InitTemplate(name, directory);
		}, nameArg, dirOption);

		var newLibCmd = new Command("lib", "Creates a sample lib(dll) tool along with the files needed to launch and debug the csharp project.");
		newCmd.Add(newLibCmd);

		var nameArgLib = new Argument<string>("name",
			"The name of the tool lib to be created.");
		newLibCmd.AddArgument(nameArgLib);

		var dirOptionLib = new Option<string>(new[] { "--directory", "-dir" }, () =>
		{
			return Directory.GetCurrentDirectory();
		}, "The directory to initialize the tool lib. Defaults to current directory.");
		newLibCmd.AddOption(dirOptionLib);
		newLibCmd.SetHandler((string name, string directory) =>
		{
			if (string.IsNullOrEmpty(name))
			{
				Console.WriteLine("please set the tool name \nusage: metatool new lib <name>");
				return;
			}

			if (!Directory.Exists(directory))
			{
				Console.WriteLine("the directory is not exist");
				return;
			}

			var scaffolder = new Scaffold(_logger);
			scaffolder.InitTemplate(name, directory, false);
		}, nameArg, dirOption);

		var runCmd = new Command("run", "run the script or lib with metatool");
		app.Add(runCmd);

		var pathArg = new Argument<string>("path", "The dir and name of the script(.csx) to be created.");
		runCmd.Add(pathArg);

		runCmd.SetHandler(path =>
		{
			if (string.IsNullOrEmpty(path))
			{
				Console.WriteLine(
					$"the 'run' command should have argument of a file end with '.csx' or '.dll'");
				return;
			}

			if (!path.EndsWith(".dll") && !path.EndsWith(".csx"))
			{
				Console.WriteLine($"The value for {path} must be end with '.csx' or '.dll'");
				return;
			}

			var fullPath = path;
			if (!File.Exists(fullPath))
			{
				fullPath = Path.Combine(Context.CurrentDirectory, fullPath);

				if (!File.Exists(fullPath))
				{
					Console.WriteLine($"the value '{path}' is not exist.");
					return;
				}
			}

			App.RunApp(() =>
            {
                if (fullPath.EndsWith(".dll"))
                {
                    try
                    {
                        Services.GetOrCreate<PluginManager>().LoadDll(fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"Error while loading tool {fullPath}! No tools loaded! Please fix it then restart!");
                    }
                }
                else if (fullPath.EndsWith(".csx"))
                {
                    var assemblyName = Path.GetFileName(Path.GetDirectoryName(fullPath));
                    _logger.LogInformation($"Compile&Run: {fullPath}, {assemblyName}");
                    Services.GetOrCreate<PluginManager>().BuildReload(fullPath, assemblyName, false);
                }
            });
		}, pathArg);

		// on windows we have command to register .csx files to be executed by dotnet-script
		var registerCmd = new Command("register", "Register .csx file handler to enable running scripts directly");
		app.Add(registerCmd);

		registerCmd.SetHandler(() =>
		{
			var scaffolder = new Scaffold(_logger);
			scaffolder.Register();
		});

		return app.InvokeAsync(_args);
	}

}