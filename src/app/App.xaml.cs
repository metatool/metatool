using System;
using System.Windows;
using Metatool.Core;
using Metatool.Service;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public partial class App : Application
    {
        private readonly IConfig<MetatoolConfig> _config;
#if DEBUG
        private static bool IsDebug = true;
#else
        private static bool IsDebug = false;
#endif
        public App(IConfig<MetatoolConfig> config)
        {
            _config = config;
        }

        private static void ConfigNotify(INotify notify)
        {
            notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    ConsoleExt.ShowConsole();
                else
                    ConsoleExt.HideConsole();
            }, null, true, IsDebug);

            notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);
            notify.ShowMessage("Metatool started!");
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.MainWindow = new MainWindow();

            var notify = Services.Get<INotify>();
            notify.ShowMessage("Metatool starting...");

            var logger = Services.Get<ILogger<App>>();

            var scaffolder = new Scaffolder(logger);
            scaffolder.CommonSetup(_config);

            ConfigNotify(notify);
            logger.LogInformation($"Registered MetatoolDir: {Environment.GetEnvironmentVariable("MetatoolPath")}");
            logger.LogInformation("Metatool started!");
        }

        internal void RunApp()
        {
            InitializeComponent();
            Run();
        }
    }
}
