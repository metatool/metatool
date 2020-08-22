using System.IO;
using Metaseed.Metatool.Service;
using Metatool.Command;
using Metatool.Core;
using Metatool.Input;
using Metatool.Service;
using Metatool.Plugins;
using Metatool.ScreenHint;
using Metatool.UI;
using Metatool.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Metaseed.Metatool.HostService;
using Metatool.UI.Notify;

namespace Metaseed.Metatool
{
    public class ServiceConfig
    {
        private static IHostBuilder ConfigHostBuilder(IHostBuilder builder) =>
            builder
                .UseContentRoot(Context.AppDirectory) // needed for locating appsettings.json when currentDir is not the metatool.exe dir, i.e. invoke from commandline
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Context.AppDirectory);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config
                        .AddPluginsConfig()
                        .AddEnvironmentVariables(prefix: "METATOOL_");
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    //loggingBuilder.AddConsole(o => o.Format = ConsoleLoggerFormat.Default);
                    // loggingBuilder.AddProvider(new TraceSourceLoggerProvider(
                    //     new SourceSwitch("sourceSwitch", "Logging Sample") {Level = SourceLevels.All},
                    //     new TextWriterTraceListener(writer: Console.Out)));
                    logging
                    //.AddProvider(new SimpleConsoleLoggerProvider())
                    .AddFile(o => o.RootPath = Context.AppDirectory)
                    ;
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .Configure<MetatoolConfig>(hostContext.Configuration)
                        .AddSingleton<IContextVariable, ContextVariable>()
                        .AddSingleton(typeof(IConfig<>), typeof(ToolConfig<>))
                        .Configure<LoggerFilterOptions>(options =>
                            options.MinLevel = hostContext.HostingEnvironment.IsDevelopment()
                                ? LogLevel.Trace
                                : LogLevel.Information)
                        .AddSingleton<IKeyboard, Keyboard>()
                        .AddSingleton<IClipboard, Clipboard>()
                        .AddSingleton<IMouse, Mouse>()
                        .AddSingleton<ICommandManager, CommandManager>()
                        .AddSingleton<INotify, Notify>()
                        .AddSingleton<IScreenHint, ScreenHint>()
                        .AddMetatoolUtils()
                        .AddPipelineBuilder()
                        .AddHostedService<StartupService>()
                        .AddHostedService<LifetimeEventsHostedService>()
                        .AddHostedService<ConsoleInputService>();
                });

        internal static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configPath = Path.Combine(Context.AppDirectory, "appsettings.Production.json");
            if (!File.Exists(configPath))
            {
                File.Copy(Path.Combine(Context.BaseDirectory, "appsettings.Production.json"), configPath);
            }

            var builder = Host.CreateDefaultBuilder(args);
            return ConfigHostBuilder(builder);
        }

        public static IHost BuildHost(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            Services.SetDefaultProvider(host.Services);
            return host;
        }
    }
}