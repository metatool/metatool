using System;
using System.Threading;
using System.Windows;
using Metatool.Core;
using Metatool.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool
{
    public partial class App : Application
    {
        private readonly IConfig<MetatoolConfig> _config;
        private readonly ILogger<App> _logger;
        private readonly IHostEnvironment _hostEnv;

        public App(IConfig<MetatoolConfig> config, IHostEnvironment hostEnv, ILogger<App> logger)
        {
            _config = config;
            _logger = logger;
            _hostEnv = hostEnv;
        }

        private void ConfigNotify(INotify notify, ILogger logger, Scaffolder scaffolder)
        {
            notify.AddContextMenuItem("Show Log", e =>
            {
                if (e.IsChecked)
                    ConsoleExt.ShowConsole();
                else
                    ConsoleExt.HideConsole();
            }, null, true, _hostEnv.IsDevelopment());

            notify.AddContextMenuItem("Auto Start", e => AutoStartManager.IsAutoStart = e.IsChecked, null, true,
                AutoStartManager.IsAutoStart);
            notify.AddContextMenuItem("Register", e =>
            {
                scaffolder.Register();
            });

            notify.ShowMessage("Metatool started!");
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.MainWindow = new MainWindow();

            var notify = Services.Get<INotify>();
            notify.ShowMessage("Metatool starting...");


            var scaffolder = new Scaffolder(_logger);
            scaffolder.CommonSetup(_config);

            ConfigNotify(notify, _logger, scaffolder);
            _logger.LogInformation($"Registered MetatoolDir: {Environment.GetEnvironmentVariable("MetatoolDir")}");
            _logger.LogInformation("Metatool started!");
        }

        internal static void RunApp()
        {
            var newWindowThread = new Thread(Start);
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        private static void Start()
        {
            var application = Services.Get<App>();
            Context.Dispatcher = application.Dispatcher;

            application.InitializeComponent();
            application.Run();
            //System.Windows.Threading.Dispatcher.Run();
        }
    }
}
