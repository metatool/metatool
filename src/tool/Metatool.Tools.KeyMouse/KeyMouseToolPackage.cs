using System.IO;
using KeyMouse;
using Metatool.Input;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Tools.KeyMouse
{
    public class KeyMouseToolPackage : CommandPackage
    {
        private readonly Engine _engine;
        private readonly MainWindow _overlayWindow;

        public IKeyCommand ActivateCommand;
        public IKeyCommand ReshowCommand;

        public KeyMouseToolPackage(IKeyboard keyboard, IConfig<PluginConfig> config)
        {
            var cfg = config.CurrentValue;
            var engineConfig = new Config { Keys = cfg.Keys };

            string modelPath = Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location)!,
                "icon_detect.onnx");

            _overlayWindow = new MainWindow();
            _engine = new Engine(modelPath, engineConfig, _overlayWindow);

            var hotkeys = cfg.Hotkeys;

            ActivateCommand = hotkeys.Activate.OnEvent(e =>
            {
                e.Handled = true;
                _engine.Activate();
            });

            ReshowCommand = hotkeys.Reshow.OnEvent(e =>
            {
                e.Handled = true;
                _engine.Reshow();
            });

            // Hook raw KeyDown for hint mode key handling
            if (keyboard is Keyboard kb)
            {
                kb.KeyDown += _engine.HandleKeyDown;
            }

            RegisterCommands();
        }

        public void OnUnloading()
        {
            ActivateCommand?.Remove();
            ReshowCommand?.Remove();
            _engine?.Dispose();
            _overlayWindow?.Close();
        }
    }
}
