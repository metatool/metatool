using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace Metaseed.Metatool
{
    public class ArgumentProcessor
    {
        const string HelpOptionTemplate = "-? | -h | --help";
        private static void ArgumentsProcess(string[] args)
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
                    "The name of the sample script file to be created during initialization.");
                var cwd = c.Option("-dir |--directory <currentworkingdirectory>",
                    "Working directory for initialization. Defaults to current directory.",
                    CommandOptionType.SingleValue);
                c.HelpOption(HelpOptionTemplate);
                c.OnExecute(() =>
                {
                    // var logFactory = CreateLogFactory(verbosity.Value(), debugMode.HasValue());
                    // new InitCommand(logFactory).Execute(new InitCommandOptions(fileName.Value, cwd.Value()));
                    // return 0;
                });
            });
        }
    }
}