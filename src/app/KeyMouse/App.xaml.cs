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
using System;
using Metatool.Plugin;
namespace KeyMouse;

public partial class App : Application
{
    private KeyMouseTool _tool;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _tool = SelfHostedTool.BuildTool<KeyMouseTool, PluginConfig>("Metatool.Tools.KeyMouse");
        var logger = Services.Get<ILogger<App>>();
        logger.LogInformation("KeyMouse Tool started");
    }

}