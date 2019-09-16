using System;
using System.Collections.Generic;
using System.Windows;
using Metaseed.Core;
using Metaseed.Metaing;
using Metaseed.MetaKeyboard;
using Metaseed.MetaPlugin;
using Metaseed.NotifyIcon;
using Metaseed.Plugin;
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
            Application.Current.MainWindow = new Settings();
            Notify.ShowMessage("MetaKeyboard started!");

            UI.Window.InitialConsole();
            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            ServiceLocator.Current = serviceCollection.BuildServiceProvider();
            var logger          = ServiceLocator.Current.GetService<ILogger<App>>();
            PluginLoad.Load(serviceCollection, logger);

            ServiceLocator.Current = serviceCollection.BuildServiceProvider();

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

            logger.LogInformation("Log in Program.cs");
            var plugins = ServiceLocator.Current.GetServices<IMetaPlugin>();
            foreach (var plugin in plugins)
            {
                plugin.Init();
            }
        }
    }
}