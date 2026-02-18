using System.IO;
using KeyMouse;
using Metatool.Service;
using Metatool.Service.MouseKey;
using static Metatool.Service.MouseKey.Key;

namespace Metatool.Tools.KeyMouse
{
    public class KeyMouseTool : ToolBase
    {
        private Engine _engine;
        private MainWindow _overlayWindow;

        public IKeyCommand ActivateCommand;
        public IKeyCommand ReshowCommand;

        public KeyMouseTool(IKeyboard keyboard, IConfig<PluginConfig> config)
        {
            var cfg = config.CurrentValue;
            var engineConfig = new Config { Keys = cfg.Keys };

            string modelPath = Path.Combine(
                Path.GetDirectoryName(GetType().Assembly.Location)!,
                "icon_detect.onnx");

            _overlayWindow = new MainWindow();
            _engine = new Engine(modelPath, engineConfig, _overlayWindow);

            // Register Ctrl+Alt+A to activate hint mode
            ActivateCommand = (Ctrl + Alt + A).OnDown(e =>
            {
                e.Handled = true;
                _engine.Activate();
            }, description: "KeyMouse: Activate hint mode");

            // Register Ctrl+Alt+S to reshow last hints
            ReshowCommand = (Ctrl + Alt + S).OnDown(e =>
            {
                e.Handled = true;
                _engine.Reshow();
            }, description: "KeyMouse: Reshow last hints");

            // Hook raw KeyDown for hint mode key handling
            var kb = keyboard as global::Metatool.Input.Keyboard;
            if (kb != null)
            {
                kb.KeyDown += _engine.HandleKeyDown;
            }

            RegisterCommands();
        }

        public override void OnUnloading()
        {
            ActivateCommand?.Remove();
            ReshowCommand?.Remove();
            _engine?.Dispose();
            _overlayWindow?.Close();
            base.OnUnloading();
        }
    }
}
