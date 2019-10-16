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
                ExtendedHelpText = "===Metaseed Metatool==="
            };

            app.Command("init", c =>
            {
                c.Description =
                    "Creates a sample script tool along with the files needed to launch and debug the script.";

                var fileName = c.Argument("name",
                    "The name of the tool script to be created during initialization.");
                var cwd = c.Option("-dir |--directory <dir>",
                    "The directory to initialize the tool scripts. Defaults to current directory.",
                    CommandOptionType.SingleValue);
                c.HelpOption(HelpOptionTemplate);
                c.OnExecute(() =>
                {
                    var scaffolder = new Scaffolder(_logger);
                    scaffolder.InitScriptTemplate(fileName.Value, cwd.Value());
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

            return app.Execute(args);
        }
    }
}