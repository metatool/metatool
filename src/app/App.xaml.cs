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
using Microsoft.Extensions.Logging.Console;

namespace Metaseed.MetaKeyboard
{

    public class CustomLoggerProvider : ILoggerProvider
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public class CustomConsoleLogger : ILogger
        {
            private readonly string _categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                //Console.WriteLine($"{logLevel}: {_categoryName}[{eventId.Id}]: {formatter(state, exception)}");
                Console.WriteLine($"{formatter(state, exception)}");

            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
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
                    loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                    //loggingBuilder.AddConsole(o=>o.Format=ConsoleLoggerFormat.Systemd);
                    //loggingBuilder.AddProvider(new ConsoleLoggerProvider());
                    loggingBuilder.AddProvider(new CustomLoggerProvider());
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
            ServiceLocator.Current = serviceProvider;
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