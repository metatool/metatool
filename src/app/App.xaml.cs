using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Metaseed.Core;
using Metaseed.Metaing;
using Metaseed.MetaPlugin;
using Metaseed.NotifyIcon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaseed.MetaKeyboard
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    loggingBuilder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
#if RELEASE
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
#endif
                .AddTransient<IMy, MyClass>()
                ;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var serviceCollection = new ServiceCollection();
            var configuration     = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            ConfigureServices(serviceCollection, configuration);
            PluginLoad.Load(serviceCollection, configuration);


            Application.Current.MainWindow = new Settings();
            Notify.ShowMessage("MetaKeyboard started!");

            UI.Window.InitialConsole();



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


            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger          = serviceProvider.GetService<ILogger<App>>();

            logger.LogInformation("Log in Program.cs");
            var plugins = serviceProvider.GetService<IEnumerable<IMetaPlugin>>();
            var myClass = serviceProvider.GetService<IMy>();

            myClass.SomeMethod();

            foreach (var plugin in plugins)
            {
                plugin.Init();
            }
        }
    }
}