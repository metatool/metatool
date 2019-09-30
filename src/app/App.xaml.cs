using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Metatool.Command;
using Metatool.Core;
using Metatool.Input;
using Metatool.MetaKeyboard;
using Metatool.Metatool;
using Metatool.Plugin;
using Metatool.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.TraceSource;

namespace Metaseed.Metatool
{
    public partial class App : Application
    {
#if DEBUG
        private static bool IsDebug = true;
#else
        private static bool IsDebug = false;
#endif

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    //loggingBuilder.AddConsole(o => o.Format = ConsoleLoggerFormat.Default);
                    // loggingBuilder.AddProvider(new TraceSourceLoggerProvider(
                    //     new SourceSwitch("sourceSwitch", "Logging Sample") {Level = SourceLevels.All},
                    //     new TextWriterTraceListener(writer: Console.Out)));
                    loggingBuilder.AddProvider(new CustomConsoleLoggerProvider());
                    loggingBuilder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
                .Configure<LoggerFilterOptions>(options =>
                    options.MinLevel = IsDebug ? LogLevel.Trace : LogLevel.Information)
                .AddSingleton<IKeyboard, Keyboard>()
                .AddSingleton<ICommandManager, CommandManager>();
        }

        private static void ConfigNotify()
        {
            Notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    ConsoleExt.ShowConsole();
                else
                    ConsoleExt.HideConsole();
            }, null, true, IsDebug);

            Notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);
            Notify.ShowMessage("Metatool started!");
        }

        void SetupEnvVar(ILogger logger)
        {
            var value = Environment.GetEnvironmentVariable("MetatoolDir");
            if (value == null)
            {
                Environment.SetEnvironmentVariable("MetatoolDir", AppContext.BaseDirectory,
                    EnvironmentVariableTarget.User);
                logger.LogInformation($"Set User Environment Var: MetatoolDir");
            }

            static string AddToPath(ILogger log, EnvironmentVariableTarget target)
            {
                var s     = System.Environment.GetEnvironmentVariable("PATH",target);
                var paths = s.Split(Path.PathSeparator);
                if (!paths.Contains(AppContext.BaseDirectory, StringComparer.InvariantCultureIgnoreCase))
                {
                    s = $"{AppContext.BaseDirectory}{Path.PathSeparator}{s}";
                    System.Environment.SetEnvironmentVariable("PATH", s, target);
                    log.LogInformation($"Add to {target} PATH Environment Var.");
                }

                return s;
            }

            var pathval = AddToPath(logger, EnvironmentVariableTarget.User);
            try
            {
                AddToPath(logger, EnvironmentVariableTarget.Machine);
            }
            catch
            {
                logger.LogWarning("Could not add to Machine scale Path environment variable!");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Notify.ShowMessage("Metatool starting...");
            Current.MainWindow = new MainWindow();
            ConsoleExt.InitialConsole(true);

            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            var provider = serviceCollection.BuildServiceProvider();
            ServiceLocator.Current = provider;
            var logger = provider.GetService<ILogger<App>>();
            SetupEnvVar(logger);

            var firstArg      = e.Args.FirstOrDefault();
            var pluginManager = ActivatorUtilities.GetServiceOrCreateInstance<PluginManager>(provider);
            if (firstArg != null)
            {
                if (firstArg.EndsWith(".dll"))
                {
                    try
                    {
                        pluginManager.LoadDll(firstArg);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error while loading tool{firstArg}! No tools loaded! Please fix it then restart!");
                    }
                }
            }
            else
            {
                pluginManager.InitPlugins();
            }

            ConfigNotify();
            logger.LogInformation($"MetatoolDir: {Environment.GetEnvironmentVariable("MetatoolPath")}");
            logger.LogInformation("Metatool started!");
        }
    }
}