using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public class ArgumentProcessor
    {
        private readonly ILogger _logger;

        public ArgumentProcessor(ILogger logger)
        {
            _logger = logger;
        }
        const string HelpOptionTemplate = "-? | -h | --help";
        public int ArgumentsProcess(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "metatool",
                Description = "tools for Windows",
                ExtendedHelpText = "===Metaseed Metatool==="
            };
            app.HelpOption(inherited: true);
            app.Command("new", configCmd =>
            {
                configCmd.OnExecute(() =>
                {
                    Console.WriteLine("Specify a subcommand");
                    configCmd.ShowHelp();
                    return 1;
                });

                configCmd.Command("script", c =>
                {
                    c.Description =
                        "Creates a sample script tool along with the files needed to launch and debug the script.";

                    var fileName = c.Argument("name",
                        "The name of the tool script to be created.");
                    var cwd = c.Option("-dir |--directory <dir>",
                        "The directory to initialize the tool scripts. Defaults to current directory.",
                        CommandOptionType.SingleValue);
                    c.HelpOption(HelpOptionTemplate);
                    c.OnExecute(() =>
                    {
                        var scaffolder = new Scaffolder(_logger);
                        scaffolder.InitTemplate(fileName.Value, cwd.Value());
                    });
                });

                configCmd.Command("lib", c =>
                {
                    c.Description =
                        "Creates a sample lib(dll) tool along with the files needed to launch and debug the project.";

                    var fileName = c.Argument("name",
                        "The name of the tool to be created.");
                    var cwd = c.Option("-dir |--directory <dir>",
                        "The directory to initialize the tool. Defaults to current directory.",
                        CommandOptionType.SingleValue);
                    c.HelpOption(HelpOptionTemplate);
                    c.OnExecute(() =>
                    {
                        var scaffolder = new Scaffolder(_logger);
                        scaffolder.InitTemplate(fileName.Value, cwd.Value(),true);
                    });
                });
            });

            // windows only 
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // on windows we have command to register .csx files to be executed by dotnet-script
                app.Command("register", c =>
                {
                    c.Description = "Register .csx file handler to enable running scripts directly";
                    c.HelpOption(HelpOptionTemplate);
                    c.OnExecute(() =>
                    {
                        var scaffolder = new Scaffolder(_logger);
                        scaffolder.RegisterFileHandler();
                    });
                });
            }

            app.OnExecute(() =>
            {
                Console.WriteLine("Specify a subcommand");
                app.ShowHelp();
                return 1;
            });

            return app.Execute(args);
        }
    }
}