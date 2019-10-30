using System.IO;
using System.Linq;
using Metaseed.Metatool.Service;
using Metatool.Command;
using Metatool.Input;
using Metatool.MetaKeyboard;
using Metatool.Metatool;
using Metatool.Plugin;
using Metatool.Plugins;
using Metatool.UI;
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
                    loggingBuilder.AddConfiguration(configuration.GetSection("Services").GetSection("Logging"));
                    //loggingBuilder.AddConsole(o => o.Format = ConsoleLoggerFormat.Default);
                    // loggingBuilder.AddProvider(new TraceSourceLoggerProvider(
                    //     new SourceSwitch("sourceSwitch", "Logging Sample") {Level = SourceLevels.All},
                    //     new TextWriterTraceListener(writer: Console.Out)));
                    loggingBuilder.AddProvider(new CustomConsoleLoggerProvider());
                    loggingBuilder.AddFile(o => o.RootPath = Context.AppDirectory);
                })
                .Configure<LoggerFilterOptions>(options =>
                    options.MinLevel = IsDebug ? LogLevel.Trace : LogLevel.Information)
                .AddSingleton<IKeyboard, Keyboard>()
                .AddSingleton<IMouse, Mouse>()
                .AddSingleton<ICommandManager, CommandManager>()
                .AddSingleton(typeof(IConfig<>), typeof(Config<>))
                .AddSingleton<INotify, Notify>()
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
            var toolIds = configuration.GetSection("Tools").GetChildren().Select(t => t.Key).ToList();
            var provider = serviceCollection.BuildServiceProvider();
            Services.SetDefaultProvider(provider);
            return provider;
        }
    }
}