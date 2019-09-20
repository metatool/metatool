using System;
using System.Collections.Generic;
using System.Windows;
using Metatool.Core;
using Metatool.Metaing;
using Metatool.MetaKeyboard;
using Metatool.MetaPlugin;
using Metatool.NotifyIcon;
using Metatool.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Metaseed.Metatool
{

    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    loggingBuilder.AddConsole(o=>o.Format=ConsoleLoggerFormat.Default);
                    //loggingBuilder.AddProvider(new ConsoleLoggerProvider());
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

            UI.Window.InitialConsole();
            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            var provider = serviceCollection.BuildServiceProvider();
            ServiceLocator.Current = provider;
            var logger          = provider.GetService<ILogger<App>>();
            var pluginManager = ActivatorUtilities.GetServiceOrCreateInstance<PluginManager>(provider);

            Notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                {
                    UI.Window.ShowConsole();
                }
                else
                {
                    UI.Window.HideConsole();
                }
            }, null, true);

            Notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);

            logger.LogInformation("Metatool started!");
            Notify.ShowMessage("Metatool started!");
        }
    }
}
