using System.IO;
using KeyMouse;
using Metatool.Input;
using Metatool.Service;
using Metatool.Service.MouseKey;

namespace Metatool.Tools.KeyMouse
{
    public class KeyMouseToolPackage : CommandPackage
    {
        private readonly KeyMouseEngine _engine;
        private readonly KeyMouseMainWindow _overlayWindow;

        public IKeyCommand ActivateCommand;
        public IKeyCommand ReshowCommand;

        public KeyMouseToolPackage(IKeyboard keyboard, IConfig<PluginConfig> config)
        {
            var cfg = config.CurrentValue;
            var engineConfig = new Config { Keys = cfg.Keys };

            _overlayWindow = new KeyMouseMainWindow();
            _engine = new KeyMouseEngine(engineConfig, _overlayWindow);

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
