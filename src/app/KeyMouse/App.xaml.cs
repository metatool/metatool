using System;
using Metatool.Input.MouseKeyHook;
using Metatool.ScreenHint;
using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using Metatool.Tools.KeyMouse;
using Microsoft.Extensions.Configuration;
using Application = System.Windows.Application;
using Mouse = Metatool.Input.Mouse;
using CommandManager = Metatool.Command.CommandManager;
namespace KeyMouse;

public partial class App : Application
{
    private ILogger<App> _logger;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build configuration
        var config = new ConfigurationBuilder()
            //.SetBasePath(AppContext.BaseDirectory)
            //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.ConfigScreenHint()
            .AddSingleton<IMouse, Mouse>()
            .AddSingleton<ICommandManager, CommandManager>();

        var serviceProvider = services.BuildServiceProvider();
        Services.SetDefaultProvider(serviceProvider);

        _logger = serviceProvider.GetRequiredService<ILogger<App>>();

        var tool = ActivatorUtilities.CreateInstance<KeyMouseTool>(serviceProvider);

    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}