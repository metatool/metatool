using System;
using System.Windows;
using Metaseed.Core;
using Metaseed.Metaing;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var serviceCollection = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("config.json")
                .Build();
            ConfigureServices(serviceCollection, configuration);
   




            Application.Current.MainWindow = new Settings();
            Notify.ShowMessage("MetaKeyboard started!");

            UI.Window.InitialConsole();
            PluginLoad.Load();

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
            var logger = serviceProvider.GetService<ILogger<App>>();

            logger.LogInformation("Log in Program.cs");
            var myClass = serviceProvider.GetService<IMy>();

            myClass.SomeMethod();

        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.AddConfiguration(configuration.GetSection("Logging"));
                    configure.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                })
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
                .AddTransient<IMy, MyClass>();
        }
    }
}