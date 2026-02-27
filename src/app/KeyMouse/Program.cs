using System;
using System.Windows;
using Metatool.Tool;
using Metatool.Service;
using Metatool.Tools.KeyMouse;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace KeyMouse;

public static class Program
{
    private static KeyMouseTool _tool;

    [STAThread]
    public static void Main()
    {
        new App().Run();
    }

    class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _tool = SelfHostedTool.BuildTool<KeyMouseTool, PluginConfig>("Metatool.Tools.KeyMouse");
            var logger = Services.Get<ILogger<KeyMouseTool>>();
            var notify = Services.Get<INotify>();
            SetupContextMenu(notify);
            logger.LogInformation("KeyMouse Tool started");
        }

        void SetupContextMenu(INotify notify)
        {
            notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    ConsoleExt.ShowConsole();
                else
                    ConsoleExt.HideConsole();
            }, null, true, Debugger.IsAttached);
        }
    }
}
