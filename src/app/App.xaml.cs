using System;
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
using Metatool.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                .AddSingleton<IMouse, Mouse>()
                .AddSingleton<ICommandManager, CommandManager>()
                .AddSingleton<INotify, Notify>();
        }

        private static void ConfigNotify(INotify notify)
        {
            notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    ConsoleExt.ShowConsole();
                else
                    ConsoleExt.HideConsole();
            }, null, true, IsDebug);

            notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);
            notify.ShowMessage("Metatool started!");
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
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            Current.MainWindow = new MainWindow();
            ConsoleExt.InitialConsole(true);

            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            var provider = serviceCollection.BuildServiceProvider();
            Services.Provider = provider;
            var notify = provider.GetService<INotify>();
            notify.ShowMessage("Metatool starting...");

            var logger = provider.GetService<ILogger<App>>();
            SetupEnvVar(logger);

            var firstArg      = e.Args.FirstOrDefault();
            var pluginManager = ActivatorUtilities.GetServiceOrCreateInstance<PluginManager>(provider);
            if (firstArg != null)
            {
                var fullPath = Path.GetFullPath(Path.Combine(currentDir,firstArg));
                if (firstArg.EndsWith(".dll"))
                {
                    try
                    {
                        pluginManager.LoadDll(fullPath);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error while loading tool{firstArg}! No tools loaded! Please fix it then restart!");
                    }
                }
                else if(firstArg.EndsWith(".csx"))
                {
                    var assemblyName = Path.GetFileName(Path.GetDirectoryName(fullPath));
                    logger.LogInformation($"{fullPath}, {assemblyName}");

                    pluginManager.BuildReload(fullPath, assemblyName, false);
                }
            }
            else
            {
                pluginManager.InitPlugins();
            }

            ConfigNotify(notify);
            logger.LogInformation($"MetatoolDir: {Environment.GetEnvironmentVariable("MetatoolPath")}");
            logger.LogInformation("Metatool started!");
        }
    }
}