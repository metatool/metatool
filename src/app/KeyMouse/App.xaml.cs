using System;
using System.Collections.Generic;
using System.Windows;
using Metatool.Input.MouseKeyHook;
using Application = System.Windows.Application;
using System.IO;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeyMouse
{
    public partial class App : Application
    {
        private IKeyboardMouseEvents _globalHook;
        private Engine _engine;
        private ILogger<App> _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();
            Services.SetDefaultProvider(serviceProvider);

            _logger = serviceProvider.GetRequiredService<ILogger<App>>();

            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon_detect.onnx");
            if (!File.Exists(modelPath))
            {
                MessageBox.Show($"Model not found: {modelPath}");
                Shutdown();
                return;
            }

            try
            {
                var config = new Config();
                var overlayWindow = new MainWindow();
                _engine = new Engine(modelPath, config, overlayWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to init engine: {ex.Message}");
                Shutdown();
                return;
            }

            _globalHook = Hook.GlobalEvents();

            _globalHook.OnCombination(new Dictionary<ICombination, Action>
            {
                { Combination.Parse("Ctrl+Alt+A"), _engine.Activate },
                { Combination.Parse("Ctrl+Alt+S"), _engine.Reshow }
            });
            _globalHook.HandleVirtualKey = true;
            _globalHook.KeyDown += _engine.HandleKeyDown;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _globalHook?.Dispose();
            _engine?.Dispose();
            base.OnExit(e);
        }
    }
}
