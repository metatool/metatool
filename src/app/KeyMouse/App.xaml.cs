using Metaseed.Metatool.Service;
using Metatool.Input;
using Metatool.ScreenHint;
using Metatool.Service;
using Metatool.Tools.KeyMouse;
using Metatool.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using Metatool.UI.Notify;
using Application = System.Windows.Application;
using CommandManager = Metatool.Command.CommandManager;
using Mouse = Metatool.Input.Mouse;
namespace KeyMouse;

public partial class App : Application
{
    private ILogger<App> _logger;
    private KeyMouseTool _tool;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build configuration
        var config = new ConfigurationBuilder()
            //.SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder =>
            builder
                //.AddConfiguration(configuration.GetSection("Logging"))
                .AddConsole()
                .AddDebug()
        );
        services
            .ConfigScreenHint()
            .ConfigMetatoolUtils()
            .AddSingleton<INotify, Notify>()
            .Configure<MetatoolConfig>(config)
            .AddSingleton(typeof(IConfig<>), typeof(ToolConfig<>))
            .Configure<PluginConfig>(config.GetSection("Tools").GetSection("Metatool.Tools.KeyMouse"))
            .AddSingleton<IKeyboard, Keyboard>()
            .AddSingleton<IMouse, Mouse>()
            .AddSingleton<ICommandManager, CommandManager>();

        var serviceProvider = services.BuildServiceProvider();
        Services.SetDefaultProvider(serviceProvider);

        _logger = serviceProvider.GetRequiredService<ILogger<App>>();
        _tool = ActivatorUtilities.CreateInstance<KeyMouseTool>(serviceProvider);

    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}