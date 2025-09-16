using Metatool.Service;
using Metatool.Service.MouseKey;
using static Metatool.Service.MouseKey.Key;

namespace Metatool.MetaKeyboard
{
    partial class Keyboard61 : CommandPackage
    {
        public Keyboard61(IKeyboard keyboard, IMouse mouse, IConfig<Config> config)
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
            SetupWinLock();
            RegisterCommands();

            var cfg  = config.CurrentValue.KeyboardPackage;
            var maps = cfg.KeyMaps;
            keyboard.RegisterKeyMaps(maps);
            var hotKeys = cfg.Hotkeys;

            hotKeys.ToggleCaps.OnEvent(e =>
            {
                e.Handled = true;
                var state = ToggleKeys.CapsLock.State;
                if (state == ToggleKeyState.AlwaysOff)
                    ToggleKeys.CapsLock.AlwaysOn();
                else if (state == ToggleKeyState.AlwaysOn)
                    ToggleKeys.CapsLock.AlwaysOff();
            });
        }

        public IKeyCommand Esc = Caps.MapOnHit(Key.Esc, e => !e.IsVirtual);
    }
}