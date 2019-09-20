using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Metatool.Core;
using Metatool.Metaing;
using Metatool.MetaKeyboard;
using Metatool.NotifyIcon;
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
        private PluginManager _pluginManager;
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    loggingBuilder.AddConsole(o => o.Format = ConsoleLoggerFormat.Default);
                    loggingBuilder.AddProvider(new TraceSourceLoggerProvider(
                        new SourceSwitch("sourceSwitch", "Logging Sample") {Level = SourceLevels.All},
                        new TextWriterTraceListener(writer: Console.Out)));
                    //loggingBuilder.AddProvider(new CustomConsoleLoggerProvider());
                    loggingBuilder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
#if RELEASE
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
#endif
                ;
        }
        private static void ConfigNotify()
        {
            Notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    global::Metatool.UI.ConsoleExt.ShowConsole();
                else
                    global::Metatool.UI.ConsoleExt.HideConsole();
            }, null, true);

            Notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);
            Notify.ShowMessage("Metatool started!");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Notify.ShowMessage("Metatool starting...");
            Current.MainWindow = new MainWindow();
            ConsoleExt.InitialConsole();

            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            var provider = serviceCollection.BuildServiceProvider();
            ServiceLocator.Current = provider;
            _pluginManager = ActivatorUtilities.GetServiceOrCreateInstance<PluginManager>(provider);
            ConfigNotify();
            var logger        = provider.GetService<ILogger<App>>();
            logger.LogInformation("Metatool started!");
        }

    }
}