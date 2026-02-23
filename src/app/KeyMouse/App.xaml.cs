using Metatool.Input.MouseKeyHook;
using Metatool.ScreenHint;
using Metatool.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using Metatool.Tools.KeyMouse;
using Application = System.Windows.Application;

namespace KeyMouse
{
    public partial class App : Application
    {
        private IKeyboardMouseEvents _globalHook;
        private ILogger<App> _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.ConfigScreenHint();

            var serviceProvider = services.BuildServiceProvider();
            Services.SetDefaultProvider(serviceProvider);

            _logger = serviceProvider.GetRequiredService<ILogger<App>>();

            var tool = ActivatorUtilities.CreateInstance<KeyMouseTool>(serviceProvider);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            _globalHook?.Dispose();
            base.OnExit(e);
        }
    }

}
