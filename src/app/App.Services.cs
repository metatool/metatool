using System;
using System.IO;
using System.Linq;
using Metaseed.Metatool.Service;
using Metatool.Command;
using Metatool.Input;
using Metatool.Metatool;
using Metatool.Service;
using Metatool.Plugins;
using Metatool.ScreenHint;
using Metatool.UI;
using Metatool.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public partial class App
    {
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddLogging(loggingBuilder =>
                {
                    var logConfig = configuration.GetSection("Services").GetSection("Logging");
                    loggingBuilder.AddConfiguration(logConfig);
                    //loggingBuilder.AddConsole(o => o.Format = ConsoleLoggerFormat.Default);
                    // loggingBuilder.AddProvider(new TraceSourceLoggerProvider(
                    //     new SourceSwitch("sourceSwitch", "Logging Sample") {Level = SourceLevels.All},
                    //     new TextWriterTraceListener(writer: Console.Out)));
                    loggingBuilder.AddProvider(new CustomConsoleLoggerProvider());
                    if (logConfig.GetSection("File").GetValue<bool>("Enabled"))
                    {
                        loggingBuilder.AddFile(o => o.RootPath = Context.AppDirectory);
                    }
                    else
                        Console.WriteLine("FileLogger is disabled, modify the config.json to enable it");
                })
                .Configure<LoggerFilterOptions>(options =>
                    options.MinLevel = IsDebug ? LogLevel.Trace : LogLevel.Information)
                .AddSingleton<IKeyboard, Keyboard>()
                .AddSingleton<IMouse, Mouse>()
                .AddSingleton<ICommandManager, CommandManager>()
                .AddSingleton(typeof(IConfig<>), typeof(Config<>))
                .AddSingleton<INotify, Notify>()
                .AddSingleton<IScreenHint, ScreenHint>()
                .AddMetatoolUtils()
                // .AddSingleton<IServiceCollection>(services)
                .AddSingleton<IConfiguration>(configuration);
        }

        internal static ServiceProvider InitServices()
        {
            var serviceCollection = new ServiceCollection();

            var configPath = Path.Combine(Context.AppDirectory, "config.json");
            if (!File.Exists(configPath))
            {
                File.Copy(Path.Combine(Context.BaseDirectory, "config.json"), configPath);
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Context.BaseDirectory, "config.default.json"), optional: false,
                    reloadOnChange: true)
                .AddPluginsConfig()
                .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                .Build();
            ConfigureServices(serviceCollection, configuration);
            var provider = serviceCollection.BuildServiceProvider();
            Services.SetDefaultProvider(provider);
            return provider;
        }
    }
}