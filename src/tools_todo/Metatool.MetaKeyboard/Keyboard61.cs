using Metatool.MetaKeyboard;
using Metatool.Service;
using static Metatool.Service.Key;

namespace ConsoleApp1
{
    partial class Keyboard61 : CommandPackage
    {
        public Keyboard61(IKeyboard keyboard, IMouse mouse, IConfig<Config> config)
        {
            ToggleKeys.NumLock.AlwaysOn();
            ToggleKeys.CapsLock.AlwaysOff();
            SetupWinLock();
            RegisterCommands();
            var cfg  = config.CurrentValue.Keyboard61Package;
            var maps = cfg.KeyMaps;
            keyboard.RegisterKeyMaps(maps);
            var hotKeys = cfg.HotKeys;

            hotKeys.ToggleCaps.Event(e =>
            {
                e.Handled = true;
                var state = ToggleKeys.CapsLock.State;
                if (state == ToggleKeyState.AlwaysOff)
                    ToggleKeys.CapsLock.AlwaysOn();
                else if (state == ToggleKeyState.AlwaysOn)
                    ToggleKeys.CapsLock.AlwaysOff();
            });
        }

        public IKeyCommand Esc = Caps.MapOnHit(Key.Esc, e => !e.IsVirtual, false);
    }
}