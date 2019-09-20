using System;
using System.Diagnostics;
using System.Windows;
using Metatool.Core;
using Metatool.Metaing;
using Metatool.MetaKeyboard;
using Metatool.NotifyIcon;
using Metatool.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.TraceSource;
using Win=Metatool.UI.Window;
namespace Metaseed.Metatool
{
    public partial class App:Application
    {
        private TaskbarIcon notifyIcon;

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    loggingBuilder.AddConsole(o => o.Format = ConsoleLoggerFormat.Default);
                    loggingBuilder.AddProvider(new TraceSourceLoggerProvider(
                        new SourceSwitch("sourceSwitch", "Logging Sample"){Level = SourceLevels.All},
                        new TextWriterTraceListener(writer: Console.Out)));
                    //loggingBuilder.AddProvider(new CustomConsoleLoggerProvider());
                    loggingBuilder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
#if RELEASE
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
#endif
                ;
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Current.MainWindow = new Settings();
            Notify.ShowMessage("Metatool starting...");

            Win.InitialConsole();
            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            var provider = serviceCollection.BuildServiceProvider();
            ServiceLocator.Current = provider;
            var logger        = provider.GetService<ILogger<App>>();
            var pluginManager = ActivatorUtilities.GetServiceOrCreateInstance<PluginManager>(provider);

            Notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    Win.ShowConsole();
                else
                    Win.HideConsole();
            }, null, true);

            Notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);

            logger.LogInformation("Metatool started!");
            Notify.ShowMessage("Metatool started!");

        }
    }
}